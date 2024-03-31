using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Configuration;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;
using User = TelegramBudget.Data.Entities.User;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class HistoryTextHandler(
    ITelegramBotClient bot,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
    : ITextHandler
{
    public bool ShouldBeInvoked(Message message)
    {
        return message.Text!.Trim().StartsWith("/history") &&
               !message.Text!.Trim().StartsWith("/history_");
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);
        var budgetName = message.Text!.Trim()["/history".Length..].Trim();

        if (await ExtractBudgetAsync(budgetName, user, cancellationToken) is not { } budget)
            return;

        if (!budget.Transactions.Any())
        {
            await bot
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    string.Format(TR.L+"NO_TRANSACTIONS", budget.Name.EscapeHtml()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        var currentAmount = 0m;
        await budget.Transactions.OrderBy(e => e.CreatedAt).SendPaginatedAsync(
            (pageBuilder, pageNumber) =>
                pageBuilder.AppendLine(
                    string.Format(TR.L+"HISTORY_INTRO", budget.Name.EscapeHtml(), pageNumber)),
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
                        TR.L+"ADDED_NOTICE", 
                        user.TimeZone == TimeSpan.Zero 
                            ? TR.L+transaction.CreatedAt+AppConfiguration.DateTimeFormat + " UTC" 
                            : TR.L+transaction.CreatedAt.Add(user.TimeZone)+AppConfiguration.DateTimeFormat, 
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

    private async Task<Budget?> ExtractBudgetAsync(string budgetName, User user, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(budgetName))
        {
            if (await db
                    .Budgets
                    .Where(e => e.Name == budgetName)
                    .ToListAsync(cancellationToken) is { Count: > 0 } budgets)
            {
                if (budgets.Count == 1) return budgets[0];

                await budgets.SendPaginatedAsync(
                    (pageBuilder, pageNumber) =>
                    {
                        pageBuilder.AppendLine(string.Format(TR.L+"CHOOSE_BUDGET_HISTORY", budgetName.EscapeHtml(), pageNumber));
                    },
                    budget => $"{budget.Name.EscapeHtml()} " +
                              "<i>(" + 
                              (budget.Owner is not { } owner
                                  ? TR.L+"OWNER_UNKNOWN"
                                  : owner.Id == currentUserService.TelegramUser.Id
                                      ? TR.L+"OWNER_YOU"
                                      : string.Format(TR.L+"OWNER_USER", budget.Owner.GetFullNameLink())) + 
                              ")</i>" +
                              " ➡️ " +
                              $"/history_{budget.Id:N}",
                    (pageBuilder, currentString) =>
                    {
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
                return null;
            }

            await bot
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    string.Format(TR.L+"BUDGET_NOT_FOUND", budgetName.EscapeHtml()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return null;
        }

        if (user.ActiveBudget is { } activeBudget)
            return activeBudget;

        await bot
            .SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                TR.L+"NO_ACTIVE_BUDGET",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        return null;
    }
}