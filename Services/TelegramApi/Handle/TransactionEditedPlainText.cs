using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Configuration;
using TelegramBudget.Data;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handle;

internal sealed class TransactionEditedPlainText(
    ITelegramBotWrapper botWrapper,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
{
    public async Task ProcessAsync(Message editedMessage, string text, CancellationToken cancellationToken)
    {
        var rawAmount = text.Split()[0];
        if (!decimal.TryParse(rawAmount, out var amount))
            return;

        var user = await db.User.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);

        var candidateTransaction = await db
            .Transaction
            .FirstOrDefaultAsync(e =>
                    e.CreatedBy == currentUserService.TelegramUser.Id &&
                    e.MessageId == editedMessage.Id,
                cancellationToken);

        if (candidateTransaction is null) return;

        var oldAmount = candidateTransaction.Amount;
        var oldComment = candidateTransaction.Comment;
        var oldBudgetSum = candidateTransaction
            .Budget
            .Transactions
            .Where(x => x.Id != candidateTransaction.Id)
            .Sum(e => e.Amount);

        candidateTransaction.Amount = amount;
        candidateTransaction.Comment = text.Length > rawAmount.Length
            ? text[rawAmount.Length..].Trim().Truncate(250)
            : null;
        db.Update(candidateTransaction);
        await db.SaveChangesAsync(cancellationToken);

        var newBudgetSum = candidateTransaction.Budget.Transactions.Sum(e => e.Amount);

        var participatingConfirmationMap = candidateTransaction.Confirmations
            .ToDictionary(
                x => x.RecipientId,
                x => x.MessageId);

        foreach (var participating in candidateTransaction.Budget.Participating)
            await botWrapper
                .SendMessage(
                    participating.UserId,
                    $"💰 <b>{candidateTransaction.Budget.Name.EscapeHtml()}</b> 💰" +
                    Environment.NewLine +
                    Environment.NewLine +
                    $"{oldBudgetSum:0.00} " +
                    (oldAmount != amount
                        ? $"<s><b>{(oldAmount >= 0 ? "➕ " + oldAmount.ToString("0.00") : "➖ " + Math.Abs(oldAmount).ToString("0.00"))}</b></s> "
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
                    string.Format(
                        TR.L + "EDITED_NOTICE",
                        user.TimeZone == TimeSpan.Zero
                            ? TR.L + candidateTransaction.CreatedAt + AppConfiguration.DateTimeFormat + " UTC"
                            : TR.L + candidateTransaction.CreatedAt.Add(user.TimeZone) +
                              AppConfiguration.DateTimeFormat,
                        currentUserService.TelegramUser.GetFullNameLink()),
                    replyParameters: participatingConfirmationMap.TryGetValue(
                        participating.UserId,
                        out var messageId)
                        ? new ReplyParameters
                        {
                            MessageId = messageId
                        }
                        : null,
                    parseMode: ParseMode.Html,
                    replyMarkup: Keyboards.ShowMainInline,
                    cancellationToken: cancellationToken);
    }
}