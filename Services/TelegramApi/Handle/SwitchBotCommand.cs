using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handle;

internal sealed class SwitchBotCommand(
    ITelegramBotWrapper botWrapper,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
{
    public async Task ProcessAsync(string data, CancellationToken cancellationToken)
    {
        var budgetName = await ExtractBudgetNameAsync(data, cancellationToken);

        if (budgetName is null) return;

        var user = await db.User.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);
        if (await GetBudgetAsync(budgetName, cancellationToken) is not { } budget)
            return;

        user.ActiveBudgetId = budget.Id;
        db.Update(user);
        await db.SaveChangesAsync(cancellationToken);

        await botWrapper
            .SendMessage(
                currentUserService.TelegramUser.Id,
                string.Format(TR.L + "SWITCHED", budget.Name.EscapeHtml()) +
                ' ' +
                "<i>(" +
                (budget.Owner is not { } owner
                    ? TR.L + "OWNER_UNKNOWN"
                    : owner.Id == currentUserService.TelegramUser.Id
                        ? TR.L + "OWNER_YOU"
                        : string.Format(TR.L + "OWNER_USER", budget.Owner.GetFullNameLink())) +
                ")</i>",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }

    private async Task<string?> ExtractBudgetNameAsync(string data, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(data)) return data;

        await botWrapper
            .SendMessage(
                currentUserService.TelegramUser.Id,
                TR.L + "SWITCH_EXAMPLE",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        return null;
    }

    private async Task<Budget?> GetBudgetAsync(
        string budgetName,
        CancellationToken cancellationToken)
    {
        if (await db
                .Budget
                .Where(e => e.Name == budgetName)
                .ToListAsync(cancellationToken) is not { Count: > 0 } budgets)
        {
            await botWrapper
                .SendMessage(
                    currentUserService.TelegramUser.Id,
                    string.Format(TR.L + "BUDGET_NOT_FOUND", budgetName.EscapeHtml()),
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
                    pageBuilder.AppendLine(string.Format(TR.L + "CHOOSE_BUDGET_SWITCH", budgetName.EscapeHtml(),
                        pageNumber));
                }, budget =>
                {
                    return $"{budget.Name.EscapeHtml()} " +
                           "<i>(" +
                           (budget.Owner is not { } owner
                               ? TR.L + "OWNER_UNKNOWN"
                               : owner.Id == currentUserService.TelegramUser.Id
                                   ? TR.L + "OWNER_YOU"
                                   : string.Format(TR.L + "OWNER_USER", budget.Owner.GetFullNameLink())) +
                           ")</i>" +
                           " ➡️ " +
                           $"/switch_{budget.Id:N}";
                }, (pageBuilder, currentString) =>
                {
                    pageBuilder.AppendLine();
                    pageBuilder.AppendLine(currentString);
                }, async (pageContent, token) =>
                    await botWrapper
                        .SendMessage(
                            currentUserService.TelegramUser.Id,
                            pageContent,
                            parseMode: ParseMode.Html,
                            cancellationToken: token), cancellationToken);
            return null;
        }

        return budgets[0];
    }
}