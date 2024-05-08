using System.Text;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handlers;

public sealed class DeleteBotCommand(
    ITelegramBotClientWrapper bot,
    ICurrentUserService currentUserService,
    ApplicationDbContext db) 
{
    public async Task ProcessAsync(string data, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);
        if (await ExtractBudgetAsync(data, user, cancellationToken) is not { } budget)
            return;

        if (budget.CreatedBy != currentUserService.TelegramUser.Id)
        {
            await bot
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    string.Format(TR.L + "DELETION_RESTRICTED", budget.Name.EscapeHtml()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        var participants = await db
            .Participating
            .Where(e => e.BudgetId == budget.Id)
            .ToListAsync(cancellationToken);

        db.Budgets.Remove(budget);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var participant in participants)
            await bot
                .SendTextMessageAsync(
                    participant.ParticipantId,
                    string.Format(TR.L + "DELETED", budget.Name.EscapeHtml(),
                        currentUserService.TelegramUser.GetFullNameLink()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
    }
    
    private async Task<Budget?> ExtractBudgetAsync(string budgetName, User user, CancellationToken cancellationToken)
    {
        var errorMessageBuilder = new StringBuilder();
        if (string.IsNullOrWhiteSpace(budgetName))
        {
            errorMessageBuilder.Clear();

            if (user.ActiveBudget is { } activeBudget)
                errorMessageBuilder.AppendLine(string.Format(TR.L + "BUDGET_REQUIRED", activeBudget.Name.EscapeHtml()));
            else
                errorMessageBuilder.AppendLine(TR.L + "NO_ACTIVE_BUDGET");

            errorMessageBuilder.AppendLine();
            errorMessageBuilder.AppendLine(TR.L + "DELETE_EXAMPLE");

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
            errorMessageBuilder.AppendLine(TR.L + "DELETE_EXAMPLE");

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
                    pageBuilder.AppendLine(string.Format(TR.L + "CHOOSE_BUDGET_DELETE", budgetName.EscapeHtml(),
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
                             $"/delete_{budget.Id:N}", (pageBuilder, currentString) =>
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