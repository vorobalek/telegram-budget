using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Extensions;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class SwitchBudgetInternalTextHandler(
    ITelegramBotClient bot,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
    : ITextHandler
{
    public bool ShouldBeInvoked(Message message)
    {
        return message.Text!.Trim().StartsWith("/switch_") &&
               Guid.TryParse(message.Text!.Trim()["/switch_".Length..], out _);
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var budgetId = Guid.Parse(message.Text!.Trim()["/switch_".Length..].Trim());

        var user = await db.Users.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);
        if (await db.Budgets.FirstOrDefaultAsync(e => e.Id == budgetId, cancellationToken) is not { } budget)
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
}