using System.Text;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;

namespace TelegramBudget.Services.TelegramApi.NewHandlers;

public class NewMainBotCommand(
    ITelegramBotClient bot,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
{
    public async Task ProcessAsync(
        CancellationToken cancellationToken,
        int? messageId = null)
    {
        var menuTextBuilder = new StringBuilder();
        
        var user = await db.Users.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);
        var userToday = DateTime.UtcNow.Add(user.TimeZone).Date;
        
        menuTextBuilder.AppendLine(string.Format(TR.L + "MENU_GREETING", user.GetFullNameLink()));
        menuTextBuilder.AppendLine();
        if (user.ActiveBudget is not { } activeBudget)
        {
            menuTextBuilder.AppendLine(TR.L + "NO_ACTIVE_BUDGET");
        }
        else
        {
            menuTextBuilder.AppendLine(
                string.Format(
                    TR.L + "MENU_ACTIVE_BUDGET", 
                    activeBudget.Name, 
                    $"{activeBudget.Transactions.Sum(x => x.Amount):0.00}"));
            menuTextBuilder.AppendLine();

            if (activeBudget
                    .Transactions
                    .Where(transaction => transaction.CreatedAt.Add(user.TimeZone).Date == userToday)
                    .OrderByDescending(transaction => transaction.CreatedAt)
                    .ToArray() is not { } todayTransactions ||
                todayTransactions.Length == 0)
            {
                menuTextBuilder.AppendLine(string.Format(TR.L + "MENU_NO_TRANSACTIONS_TODAY", activeBudget.Name));
            }
            else
            {
                menuTextBuilder.AppendLine(
                    todayTransactions
                        .CreatePage(
                            1024,
                            1,
                            (builder, _) =>
                            {
                                builder.AppendLine(string.Format(TR.L + "MENU_TRANSACTIONS_TODAY", activeBudget.Name));
                                builder.AppendLine();
                            },
                            transaction => $"<b>{(
                                transaction.Amount >= 0
                                    ? "➕ " + transaction.Amount.ToString("0.00")
                                    : "➖ " + Math.Abs(transaction.Amount).ToString("0.00")
                            )}</b> <i>{transaction.Comment?.EscapeHtml() ?? string.Empty}</i>",
                            (builder, currentString) => builder.AppendLine(currentString),
                            out _,
                            out _));
            }
        }

        if (messageId.HasValue)
            await bot.EditMessageTextAsync(
                chatId: currentUserService.TelegramUser.Id,
                text: menuTextBuilder.ToString(),
                messageId: messageId.Value,
                parseMode: ParseMode.Html,
                replyMarkup: Keyboards.MenuInline,
                cancellationToken: cancellationToken
            );

        else
            await bot.SendTextMessageAsync(
                chatId: currentUserService.TelegramUser.Id,
                text: menuTextBuilder.ToString(),
                parseMode: ParseMode.Html,
                replyMarkup: Keyboards.MenuInline,
                cancellationToken: cancellationToken);
    }
}