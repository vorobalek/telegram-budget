using System.Text;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;
using User = TelegramBudget.Data.Entities.User;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class DeleteBudgetTextHandler(
    ITelegramBotClient bot,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
    : ITextHandler
{
    public bool ShouldBeInvoked(Message message)
    {
        return message.Text!.Trim().StartsWith("/delete");
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);
        if (await ExtractBudgetAsync(message, user, cancellationToken) is not { } budget)
            return;

        if (budget.CreatedBy != currentUserService.TelegramUser.Id)
        {
            await bot
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    string.Format(TR.L+"DELETION_RESTRICTED", budget.Name.EscapeHtml()),
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
                    string.Format(TR.L+"DELETED", budget.Name.EscapeHtml(), currentUserService.TelegramUser.GetFullNameLink()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
    }

    private async Task<Budget?> ExtractBudgetAsync(Message message, User user, CancellationToken cancellationToken)
    {
        var errorMessageBuilder = new StringBuilder();
        errorMessageBuilder.AppendLine((TR.L + "DELETE").EscapeHtml());
        errorMessageBuilder.AppendLine();

        var budgetName = message.Text!.Trim()["/delete".Length..].Trim();
        if (!string.IsNullOrWhiteSpace(budgetName))
        {
            if (await db.Budgets.FirstOrDefaultAsync(e => e.Name == budgetName, cancellationToken) is { } budget)
                return budget;

            errorMessageBuilder.AppendLine(string.Format(TR.L+"BUDGET_NOT_FOUND", budgetName.EscapeHtml()));
        }
        else
        {
            if (user.ActiveBudget is { } activeBudget)
                errorMessageBuilder.AppendLine(string.Format(TR.L + "BUDGET_REQUIRED", activeBudget.Name.EscapeHtml()));
            else
                errorMessageBuilder.AppendLine(TR.L + "NO_ACTIVE_BUDGET");
        }

        await bot
            .SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                errorMessageBuilder.ToString(),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        return null;
    }
}