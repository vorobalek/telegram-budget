using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Configuration;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handle;

internal sealed class TransactionPlainText(
    ITelegramBotWrapper botWrapper,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
{
    public async Task ProcessAsync(
        Message message,
        string text,
        CancellationToken cancellationToken)
    {
        var rawAmount = text.Split()[0];
        if (!decimal.TryParse(rawAmount, out var amount))
            return;

        var user = await db.User.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);

        if (user.ActiveBudget is null)
        {
            await botWrapper
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    TR.L + "NO_ACTIVE_BUDGET",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        var oldBudgetSum = user.ActiveBudget.Transactions.Sum(e => e.Amount);

        var newTransaction = new Transaction
        {
            Amount = amount,
            BudgetId = user.ActiveBudget.Id,
            MessageId = message.MessageId,
            Comment = text.Length > rawAmount.Length
                ? text[rawAmount.Length..].Trim().Truncate(250)
                : null
        };

        await db.Transaction.AddAsync(newTransaction, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var newBudgetSum = user.ActiveBudget.Transactions.Sum(e => e.Amount);

        var needSaveChange = false;
        foreach (var participating in user.ActiveBudget.Participating)
        {
            var confirmationMessage = await botWrapper
                .SendTextMessageAsync(
                    participating.UserId,
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
                    string.Format(
                        TR.L + "ADDED_NOTICE",
                        user.TimeZone == TimeSpan.Zero
                            ? TR.L + newTransaction.CreatedAt + AppConfiguration.DateTimeFormat + " UTC"
                            : TR.L + newTransaction.CreatedAt.Add(user.TimeZone) + AppConfiguration.DateTimeFormat,
                        currentUserService.TelegramUser.GetFullNameLink()),
                    replyToMessageId: participating.UserId == currentUserService.TelegramUser.Id
                        ? message.MessageId
                        : null,
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            await db.TransactionConfirmation.AddAsync(new TransactionConfirmation
            {
                MessageId = confirmationMessage.MessageId,
                RecipientId = participating.UserId,
                Transaction = newTransaction
            }, cancellationToken);
            needSaveChange = true;
        }

        if (needSaveChange)
            await db.SaveChangesAsync(cancellationToken);
    }
}