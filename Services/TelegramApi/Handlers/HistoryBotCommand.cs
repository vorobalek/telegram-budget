using System.Text;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Configuration;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handlers;

public sealed class HistoryBotCommand(
    ITelegramBotClientWrapper bot,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
{
    public async Task ProcessAsync(string data, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);

        if (await ExtractBudgetAsync(data, user, cancellationToken) is not { } budget)
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
            4096,
            (pageBuilder, pageNumber) =>
                pageBuilder.AppendLine(
                    string.Format(TR.L + "HISTORY_INTRO", budget.Name.EscapeHtml(), pageNumber)), transaction =>
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
            }, (pageBuilder, currentString) =>
            {
                pageBuilder.AppendLine();
                pageBuilder.AppendLine();
                pageBuilder.AppendLine(currentString);
            }, async (pageContent, token) =>
                await bot
                    .SendTextMessageAsync(
                        currentUserService.TelegramUser.Id,
                        pageContent,
                        parseMode: ParseMode.Html,
                        cancellationToken: token), cancellationToken);
    }
    
    private async Task<Budget?> ExtractBudgetAsync(string budgetName, User user, CancellationToken cancellationToken)
    {
        var errorMessageBuilder = new StringBuilder();
        if (string.IsNullOrWhiteSpace(budgetName))
        {
            if (user.ActiveBudget is { } activeBudget)
                return activeBudget;

            errorMessageBuilder.Clear();
            errorMessageBuilder.AppendLine(TR.L + "NO_ACTIVE_BUDGET");
            errorMessageBuilder.AppendLine();
            errorMessageBuilder.AppendLine(TR.L + "HISTORY_EXAMPLE");

            await bot
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    errorMessageBuilder.ToString(),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return null;
        }

        if (await db
                .Budgets
                .Where(e => e.Name == budgetName)
                .ToListAsync(cancellationToken) is not { Count: > 0 } budgets)
        {
            errorMessageBuilder.Clear();
            errorMessageBuilder.AppendLine(string.Format(TR.L + "BUDGET_NOT_FOUND", budgetName.EscapeHtml()));
            errorMessageBuilder.AppendLine();
            errorMessageBuilder.AppendLine(TR.L + "HISTORY_EXAMPLE");

            await bot
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    errorMessageBuilder.ToString(),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return null;
        }

        if (budgets.Count != 1)
        {
            await budgets.SendPaginatedAsync(
                4096,
                (pageBuilder, pageNumber) =>
                {
                    pageBuilder.AppendLine(string.Format(TR.L + "CHOOSE_BUDGET_HISTORY", budgetName.EscapeHtml(),
                        pageNumber));
                }, budget => $"{budget.Name.EscapeHtml()} " +
                             "<i>(" +
                             (budget.Owner is not { } owner
                                 ? TR.L + "OWNER_UNKNOWN"
                                 : owner.Id == currentUserService.TelegramUser.Id
                                     ? TR.L + "OWNER_YOU"
                                     : string.Format(TR.L + "OWNER_USER", budget.Owner.GetFullNameLink())) +
                             ")</i>" +
                             " ➡️ " +
                             $"/history_{budget.Id:N}", (pageBuilder, currentString) =>
                {
                    pageBuilder.AppendLine();
                    pageBuilder.AppendLine(currentString);
                }, async (pageContent, token) =>
                    await bot
                        .SendTextMessageAsync(
                            currentUserService.TelegramUser.Id,
                            pageContent,
                            parseMode: ParseMode.Html,
                            cancellationToken: token), cancellationToken);
            return null;
        }

        return budgets[0];
    }
}