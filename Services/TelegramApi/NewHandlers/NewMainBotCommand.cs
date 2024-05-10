using System.Text;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.DateTimeProvider;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.NewHandlers;

public class NewMainBotCommand(
    ITelegramBotClientWrapper botWrapper,
    ICurrentUserService currentUserService,
    ApplicationDbContext db,
    IDateTimeProvider dateTime)
{
    public async Task ProcessAsync(
        int? callbackQueryMessageId,
        CancellationToken cancellationToken)
    {
        var (activeBudgetId, timeZone, userUrl) = await GetUserDataAsync(cancellationToken);
        var reply = await PrepareRelyAsync(
            activeBudgetId,
            timeZone,
            userUrl,
            cancellationToken);

        await SubmitReplyAsync(
            callbackQueryMessageId,
            reply,
            cancellationToken);
    }

    private async Task<string> PrepareRelyAsync(
        Guid? budgetId,
        TimeSpan timeZone,
        string userUrl,
        CancellationToken cancellationToken)
    {
        var userToday = dateTime.UtcNow().DateTime.Add(timeZone).Date;

        var menuTextBuilder = new StringBuilder();
        menuTextBuilder.AppendLine(
            string.Format(
                TR.L + "MAIN_GREETING",
                userUrl));
        menuTextBuilder.AppendLine();

        if (!budgetId.HasValue ||
            await GetBudgetNameAsync(budgetId.Value, cancellationToken) is not { } budgetName)
        {
            menuTextBuilder.AppendLine(TR.L + "NO_ACTIVE_BUDGET");
            return menuTextBuilder.ToString();
        }

        var transactions = await GetTransactionsReversedAsync(budgetId.Value, cancellationToken);
        
        menuTextBuilder.AppendLine(
            string.Format(
                TR.L + "MAIN_ACTIVE_BUDGET",
                budgetName,
                $"{transactions.Sum(x => x.Amount):0.00}"));
        menuTextBuilder.AppendLine();

        if (transactions
                .Where(transaction => transaction.CreatedAt.Add(timeZone).Date == userToday)
                .OrderByDescending(transaction => transaction.CreatedAt)
                .ToArray() is not { } todayTransactions ||
            todayTransactions.Length == 0)
        {
            menuTextBuilder.AppendLine(
                string.Format(
                    TR.L + "MAIN_NO_TRANSACTIONS_TODAY",
                    budgetName));
            return menuTextBuilder.ToString();
        }

        menuTextBuilder.AppendLine(
            todayTransactions
                .CreatePage(
                    1024,
                    1,
                    (builder, _) =>
                    {
                        builder.AppendLine(string.Format(TR.L + "MAIN_TRANSACTIONS_TODAY", budgetName));
                    },
                    transaction => $"<b>{(transaction.Amount >= 0
                        ? $"➕ {transaction.Amount:0.00}"
                        : $"➖ {Math.Abs(transaction.Amount):0.00}")}</b> <i>{transaction.Comment?.EscapeHtml() ?? string.Empty}</i>",
                    (builder, currentString) => builder.AppendLine(currentString),
                    out _,
                    out _));

        return menuTextBuilder.ToString();
    }

    private async Task<(Guid? ActiveBudgetId, TimeSpan TimeZone, string Url)> GetUserDataAsync(
        CancellationToken cancellationToken)
    {
        var data = await db.Users
            .Where(e => e.Id == currentUserService.TelegramUser.Id)
            .Select(e => new
            {
                e.ActiveBudgetId,
                e.TimeZone,
                Url = TelegramHelper.GetFullNameLink(e.Id, e.FirstName, e.LastName)
            })
            .SingleAsync(cancellationToken);

        return (data.ActiveBudgetId, data.TimeZone, data.Url);
    }

    private Task<string?> GetBudgetNameAsync(Guid budgetId, CancellationToken cancellationToken)
    {
        return db.Budgets
            .Where(e => e.Id == budgetId)
            .Select(e => e.Name)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task<
        ICollection<(
            decimal Amount,
            string? Comment,
            DateTime CreatedAt)>
    > GetTransactionsReversedAsync(Guid budgetId, CancellationToken cancellationToken)
    {
        var data = await db.Transactions
            .Where(e => e.BudgetId == budgetId)
            .Include(e => e.Author)
            .Select(e => new
            {
                e.Amount,
                e.Comment,
                e.CreatedAt
            })
            .OrderByDescending(e => e.CreatedAt)
            .ToArrayAsync(cancellationToken);

        return data.Select(e =>
            {
                var transaction = (
                    e.Amount,
                    e.Comment,
                    e.CreatedAt);
                return transaction;
            })
            .OrderByDescending(e => e.CreatedAt)
            .ToArray();
    }

    private async Task SubmitReplyAsync(
        int? callbackQueryMessageId,
        string reply,
        CancellationToken cancellationToken)
    {
        if (callbackQueryMessageId.HasValue)
            await botWrapper.EditMessageTextAsync(
                currentUserService.TelegramUser.Id,
                text: reply,
                messageId: callbackQueryMessageId.Value,
                parseMode: ParseMode.Html,
                replyMarkup: Keyboards.MenuInline,
                cancellationToken: cancellationToken
            );

        else
            await botWrapper.SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                reply,
                parseMode: ParseMode.Html,
                replyMarkup: Keyboards.MenuInline,
                cancellationToken: cancellationToken);
    }
}