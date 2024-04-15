using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Configuration;
using TelegramBudget.Data;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;

namespace TelegramBudget.Services.TelegramApi.Handlers;

public sealed class HistoryPrefixBotCommand(
    ITelegramBotClient bot,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
{
    public async Task ProcessAsync(string botCommandPostfix, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(botCommandPostfix, out var budgetId))
            return;

        var user = await db.Users.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);
        if (await db.Budgets.FirstOrDefaultAsync(e => e.Id == budgetId, cancellationToken) is not { } budget)
            return;

        if (!budget.Transactions.Any())
        {
            await bot
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    string.Format(TR.L + "NO_TRANSACTIONS", budget.Name.EscapeHtml()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        var currentAmount = 0m;
        await budget.Transactions.OrderBy(e => e.CreatedAt).SendPaginatedAsync(
            (pageBuilder, pageNumber) =>
                pageBuilder.AppendLine(
                    string.Format(TR.L + "HISTORY_INTRO", budget.Name.EscapeHtml(), pageNumber)),
            transaction =>
            {
                var currentString =
                    $"{currentAmount:0.00} " +
                    $"<b>{(transaction.Amount >= 0 ? "➕ " + transaction.Amount.ToString("0.00") : "➖ " + Math.Abs(transaction.Amount).ToString("0.00"))}</b> " +
                    $"➡️ {currentAmount + transaction.Amount:0.00}" +
                    (transaction.Comment is not null
                        ? Environment.NewLine +
                          Environment.NewLine +
                          transaction.Comment.EscapeHtml()
                        : string.Empty) +
                    Environment.NewLine +
                    Environment.NewLine +
                    "<i>" +
                    string.Format(
                        TR.L + "ADDED_NOTICE",
                        user.TimeZone == TimeSpan.Zero
                            ? TR.L + transaction.CreatedAt + AppConfiguration.DateTimeFormat + " UTC"
                            : TR.L + transaction.CreatedAt.Add(user.TimeZone) + AppConfiguration.DateTimeFormat,
                        transaction.Author.GetFullNameLink()) +
                    "</i>";
                currentAmount += transaction.Amount;
                return currentString;
            },
            (pageBuilder, currentString) =>
            {
                pageBuilder.AppendLine();
                pageBuilder.AppendLine();
                pageBuilder.AppendLine(currentString);
            },
            async (pageContent, token) =>
                await bot
                    .SendTextMessageAsync(
                        currentUserService.TelegramUser.Id,
                        pageContent,
                        parseMode: ParseMode.Html,
                        cancellationToken: token),
            4096,
            cancellationToken);
    }
}