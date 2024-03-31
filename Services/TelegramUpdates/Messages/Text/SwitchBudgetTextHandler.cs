using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class SwitchBudgetTextHandler(
    ITelegramBotClient bot,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
    : ITextHandler
{
    public bool ShouldBeInvoked(Message message)
    {
        return message.Text!.Trim().StartsWith("/switch") &&
               !message.Text!.Trim().StartsWith("/switch_");
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var budgetName = await ExtractBudgetNameAsync(message, cancellationToken);

        if (budgetName is null) return;

        var user = await db.Users.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);
        if (await GetBudgetAsync(budgetName, cancellationToken) is not { } budget)
            return;

        user.ActiveBudgetId = budget.Id;
        db.Users.Update(user);
        await db.SaveChangesAsync(cancellationToken);

        await bot
            .SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                string.Format(TR.L+"SWITCHED", budget.Name.EscapeHtml()) +
                ' ' +
                "<i>(" + 
                (budget.Owner is not { } owner
                    ? TR.L+"OWNER_UNKNOWN"
                    : owner.Id == currentUserService.TelegramUser.Id
                        ? TR.L+"OWNER_YOU"
                        : string.Format(TR.L+"OWNER_USER", budget.Owner.GetFullNameLink())) + 
                ")</i>",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }

    private async Task<string?> ExtractBudgetNameAsync(Message message, CancellationToken cancellationToken)
    {
        var budgetName = message.Text!.Trim()["/switch".Length..].Trim();
        if (!string.IsNullOrWhiteSpace(budgetName)) return budgetName;

        await bot
            .SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                (TR.L + "SWITCH").EscapeHtml(),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        return null;
    }

    private async Task<Budget?> GetBudgetAsync(
        string budgetName,
        CancellationToken cancellationToken)
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
                    pageBuilder.AppendLine(string.Format(TR.L+"CHOOSE_BUDGET_SWITCH", budgetName.EscapeHtml(), pageNumber));
                },
                budget =>
                {
                    return $"{budget.Name.EscapeHtml()} " +
                           "<i>(" + 
                           (budget.Owner is not { } owner
                               ? TR.L+"OWNER_UNKNOWN"
                               : owner.Id == currentUserService.TelegramUser.Id
                                   ? TR.L+"OWNER_YOU"
                                   : string.Format(TR.L+"OWNER_USER", budget.Owner.GetFullNameLink())) + 
                           ")</i>" +
                           " ➡️ " +
                           $"/switch_{budget.Id:N}";
                },
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
}