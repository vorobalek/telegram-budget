using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handle;

internal sealed class DeletePrefixBotCommand(
    ITelegramBotWrapper botWrapper,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
{
    public async Task ProcessAsync(string botCommandPostfix, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(botCommandPostfix, out var budgetId))
            return;

        if (await db.Budget.FirstOrDefaultAsync(e => e.Id == budgetId, cancellationToken) is not { } budget)
            return;

        if (budget.CreatedBy != currentUserService.TelegramUser.Id)
        {
            await botWrapper
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    string.Format(TR.L + "DELETION_RESTRICTED", budget.Name.EscapeHtml()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        var participants = await db
            .Participant
            .Where(e => e.BudgetId == budget.Id)
            .ToListAsync(cancellationToken);

        db.Budget.Remove(budget);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var participant in participants)
            await botWrapper
                .SendTextMessageAsync(
                    participant.UserId,
                    string.Format(TR.L + "DELETED", budget.Name.EscapeHtml(),
                        currentUserService.TelegramUser.GetFullNameLink()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
    }
}