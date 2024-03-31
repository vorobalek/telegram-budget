using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class CreateBudgetTextHandler(
    ITelegramBotClient bot,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
    : ITextHandler
{
    public bool ShouldBeInvoked(Message message)
    {
        return message.Text!.Trim().StartsWith("/create");
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var budgetName = await ExtractBudgetNameAsync(message, cancellationToken);

        if (budgetName is null) return;

        if (await BudgetAlreadyExistsAsync(budgetName, cancellationToken))
            return;

        var user = await db.Users.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);
        var newBudget = new Budget
        {
            Name = budgetName,
            ActiveUsers = new[]
            {
                user
            }
        };
        await db.Budgets.AddAsync(newBudget, cancellationToken);

        var participating = new Participating
        {
            Budget = newBudget,
            Participant = user
        };
        await db.Participating.AddAsync(participating, cancellationToken);

        user.ActiveBudget = newBudget;
        db.Update(user);

        await db.SaveChangesAsync(cancellationToken);

        await bot
            .SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                string.Format(TR.L+"CREATED", budgetName.EscapeHtml()),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }

    private async Task<string?> ExtractBudgetNameAsync(Message message, CancellationToken cancellationToken)
    {
        var budgetName = message.Text!.Trim()["/create".Length..].Trim().Truncate(250);
        if (!string.IsNullOrWhiteSpace(budgetName)) return budgetName;

        await bot
            .SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                (TR.L+"CREATE").EscapeHtml(),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        return null;
    }

    private async Task<bool> BudgetAlreadyExistsAsync(string budgetName, CancellationToken cancellationToken)
    {
        var budgetAlreadyExists = await db.Budgets.AnyAsync(e => e.Name == budgetName, cancellationToken);
        if (!budgetAlreadyExists) return false;

        await bot
            .SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                string.Format(TR.L+"ALREADY_EXISTS", budgetName.EscapeHtml()),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        return true;
    }
}