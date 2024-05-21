using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handle;

internal sealed class CreateBotCommand(
    ITelegramBotWrapper botWrapper,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
{
    public async Task ProcessAsync(string data, CancellationToken cancellationToken)
    {
        var budgetName = await ExtractBudgetNameAsync(data, cancellationToken);

        if (budgetName is null) return;

        if (await BudgetAlreadyExistsAsync(budgetName, cancellationToken))
            return;

        var user = await db.User.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);
        var newBudget = new Budget
        {
            Name = budgetName,
            ActiveUsers = [user]
        };
        await db.Budget.AddAsync(newBudget, cancellationToken);

        var participating = new Participant
        {
            Budget = newBudget,
            User = user
        };
        await db.Participant.AddAsync(participating, cancellationToken);

        user.ActiveBudget = newBudget;
        db.Update(user);

        await db.SaveChangesAsync(cancellationToken);

        await botWrapper
            .SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                string.Format(TR.L + "CREATED", budgetName.EscapeHtml()),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }

    private async Task<string?> ExtractBudgetNameAsync(string data, CancellationToken cancellationToken)
    {
        var budgetName = data.Trim().Truncate(250);
        if (!string.IsNullOrWhiteSpace(budgetName)) return budgetName;

        await botWrapper
            .SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                TR.L + "CREATE_EXAMPLE",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        return null;
    }

    private async Task<bool> BudgetAlreadyExistsAsync(string budgetName, CancellationToken cancellationToken)
    {
        var budgetAlreadyExists = await db.Budget.AnyAsync(e => e.Name == budgetName, cancellationToken);
        if (!budgetAlreadyExists) return false;

        await botWrapper
            .SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                string.Format(TR.L + "ALREADY_EXISTS", budgetName.EscapeHtml()),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        return true;
    }
}