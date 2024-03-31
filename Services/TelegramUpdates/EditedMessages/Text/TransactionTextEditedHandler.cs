using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Configuration;
using TelegramBudget.Data;
using TelegramBudget.Extensions;

namespace TelegramBudget.Services.TelegramUpdates.EditedMessages.Text;

public class TransactionTextEditedHandler(
    ITelegramBotClient bot,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
    : ITextEditedHandler
{
    public bool ShouldBeInvoked(Message message)
    {
        return decimal.TryParse(message.Text!.Trim().Split()[0], out _);
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);

        var candidateTransaction = await db
            .Transactions
            .FirstOrDefaultAsync(e =>
                    e.CreatedBy == currentUserService.TelegramUser.Id &&
                    e.MessageId == message.MessageId,
                cancellationToken);

        if (candidateTransaction is null) return;

        var oldAmount = candidateTransaction.Amount;
        var oldComment = candidateTransaction.Comment;
        var rawAmount = message.Text!.Trim().Split()[0];
        var amount = decimal.Parse(rawAmount);
        var oldBudgetSum = candidateTransaction.Budget.Transactions.Sum(e => e.Amount);

        candidateTransaction.Amount = amount;
        candidateTransaction.Comment = message.Text!.Trim().Length > rawAmount.Length
            ? message.Text!.Trim()[rawAmount.Length..].Trim().Truncate(250)
            : null;
        db.Transactions.Update(candidateTransaction);
        await db.SaveChangesAsync(cancellationToken);

        var newBudgetSum = candidateTransaction.Budget.Transactions.Sum(e => e.Amount);

        var participatingConfirmationMap = candidateTransaction.Confirmations
            .ToDictionary(
                x => x.RecipientId,
                x => x.MessageId);

        foreach (var participating in candidateTransaction.Budget.Participating)
            await bot
                .SendTextMessageAsync(
                    participating.ParticipantId,
                    $"💰 <b>{candidateTransaction.Budget.Name.EscapeHtml()}</b> 💰" +
                    Environment.NewLine +
                    Environment.NewLine +
                    $"{oldBudgetSum:0.00} " +
                    (oldAmount != amount
                        ? $"<b>{(oldAmount < 0 ? "➕ <s>" + Math.Abs(oldAmount).ToString("0.00") : "➖ <s>" + oldAmount.ToString("0.00"))}</s></b>  "
                        : string.Empty) +
                    $"<b>{(amount >= 0 ? "➕ " + amount.ToString("0.00") : "➖ " + Math.Abs(amount).ToString("0.00"))}</b> " +
                    $"➡️ {newBudgetSum:0.00}" +
                    (oldComment is not null || candidateTransaction.Comment is not null
                        ? Environment.NewLine +
                          Environment.NewLine
                        : string.Empty) +
                    (oldComment is not null && oldComment != candidateTransaction.Comment
                        ? $"<s>{oldComment.EscapeHtml()}</s>"
                        : string.Empty) +
                    (oldComment is not null && candidateTransaction.Comment is not null &&
                     oldComment != candidateTransaction.Comment
                        ? " ➡️ "
                        : string.Empty) +
                    (candidateTransaction.Comment ?? string.Empty) +
                    Environment.NewLine +
                    Environment.NewLine +
                    $"<i>{TR.L+"EDITED_NOTICE"} {(user.TimeZone == TimeSpan.Zero 
                        ? TR.L+candidateTransaction.CreatedAt+AppConfiguration.DateTimeFormat + " UTC" 
                        : TR.L+candidateTransaction.CreatedAt.Add(user.TimeZone)+AppConfiguration.DateTimeFormat)} " +
                    $"{currentUserService.TelegramUser.GetFullNameLink()}</i>",
                    replyToMessageId: participatingConfirmationMap.TryGetValue(participating.ParticipantId,
                        out var messageId)
                        ? messageId
                        : null,
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
    }
}