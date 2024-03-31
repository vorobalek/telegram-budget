using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Extensions;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class ListBudgetTextHandler(
    ITelegramBotClient bot,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
    : ITextHandler
{
    public bool ShouldBeInvoked(Message message)
    {
        return message.Text!.Trim().StartsWith("/list");
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);
        var budgets = await db
            .Budgets
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.Owner,
                Sum = e.Transactions.Sum(t => t.Amount)
            })
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);

        if (!budgets.Any())
        {
            await bot
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    TR.L+"NO_BUDGETS",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        await budgets.SendPaginatedAsync(
            (pageBuilder, pageNumber) =>
            {
                pageBuilder.Append(string.Format(TR.L+"LIST_INTRO", pageNumber));
            },
            budget =>
            {
                var (@in, @out) = user.ActiveBudgetId == budget.Id
                    ? ("<b>", "</b>")
                    : (string.Empty, string.Empty);

                return $"{@in}{budget.Name.EscapeHtml()}{@out} " +
                       $"➡️ {@in}{budget.Sum:0.00}{@out} " +
                       "<i>(" +
                       (budget.Owner is not { } owner
                           ? TR.L+"OWNER_UNKNOWN"
                           : owner.Id == currentUserService.TelegramUser.Id
                               ? TR.L+"OWNER_YOU"
                               : string.Format(TR.L+"OWNER_USER", budget.Owner.GetFullNameLink())) + 
                       ")</i>";
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
    }
}