using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handlers;

public sealed class SwitchPrefixBotCommand(
    ITelegramBotClientWrapper bot,
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

        user.ActiveBudgetId = budget.Id;
        db.Users.Update(user);
        await db.SaveChangesAsync(cancellationToken);

        await bot
            .SendTextMessageAsync(
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
}