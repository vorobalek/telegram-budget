using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Configuration;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class TransactionTextHandler(
    ITelegramBotClient bot,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
    : ITextHandler
{
    public bool ShouldBeInvoked(Message message)
    {
        return decimal.TryParse(message.Text!.Trim().Split()[0], out _);
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);

        if (user.ActiveBudget is null)
        {
            await bot
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    TR.L+"NO_ACTIVE_BUDGET",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        var rawAmount = message.Text!.Trim().Split()[0];
        var amount = decimal.Parse(rawAmount);
        var oldBudgetSum = user.ActiveBudget.Transactions.Sum(e => e.Amount);

        var newTransaction = new Transaction
        {
            Amount = amount,
            BudgetId = user.ActiveBudget.Id,
            MessageId = message.MessageId,
            Comment = message.Text!.Trim().Length > rawAmount.Length
                ? message.Text!.Trim()[rawAmount.Length..].Trim().Truncate(250)
                : null
        };

        await db.Transactions.AddAsync(newTransaction, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var newBudgetSum = user.ActiveBudget.Transactions.Sum(e => e.Amount);

        var needSaveChange = false;
        foreach (var participating in user.ActiveBudget.Participating)
        {
            var confirmationMessage = await bot
                .SendTextMessageAsync(
                    participating.ParticipantId,
                    $"💰 <b>{user.ActiveBudget.Name.EscapeHtml()}</b> 💰" +
                    Environment.NewLine +
                    Environment.NewLine +
                    $"{oldBudgetSum:0.00} " +
                    $"<b>{(amount >= 0 ? "➕ " + amount.ToString("0.00") : "➖ " + Math.Abs(amount).ToString("0.00"))}</b> " +
                    $"➡️ {newBudgetSum:0.00}" +
                    (newTransaction.Comment is not null
                        ? Environment.NewLine +
                          Environment.NewLine +
                          newTransaction.Comment.EscapeHtml()
                        : string.Empty) +
                    Environment.NewLine +
                    Environment.NewLine +
                    "<i>" +
                    string.Format(
                        TR.L+"ADDED_NOTICE", 
                        user.TimeZone == TimeSpan.Zero 
                            ? TR.L+newTransaction.CreatedAt+AppConfiguration.DateTimeFormat + " UTC" 
                            : TR.L+newTransaction.CreatedAt.Add(user.TimeZone)+AppConfiguration.DateTimeFormat, 
                        currentUserService.TelegramUser.GetFullNameLink()) +
                    "</i>",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            await db.TransactionConfirmations.AddAsync(new TransactionConfirmation
            {
                MessageId = confirmationMessage.MessageId,
                RecipientId = participating.ParticipantId,
                Transaction = newTransaction
            }, cancellationToken);
            needSaveChange = true;
        }

        if (needSaveChange)
            await db.SaveChangesAsync(cancellationToken);
    }
}