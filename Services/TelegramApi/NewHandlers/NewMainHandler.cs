using System.Text;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.DateTimeProvider;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.NewHandlers;

public class NewMainHandler(
    ITelegramBotClientWrapper botWrapper,
    ICurrentUserService currentUserService,
    ApplicationDbContext db,
    IDateTimeProvider dateTime) : IBotCommandHandler, ICallbackQueryHandler
{
    public const string Command = "main";

    public async Task ProcessAsync(string _, CancellationToken cancellationToken)
    {
        var (activeBudgetId, timeZone, userUrl) = await GetUserDataAsync(cancellationToken);
        var text = await PrepareRelyAsync(
            activeBudgetId,
            timeZone,
            userUrl,
            cancellationToken);

        await SubmitReplyAsync(
            text,
            activeBudgetId.HasValue,
            cancellationToken);
    }

    public async Task ProcessAsync(int messageId, string _, CancellationToken cancellationToken)
    {
        var (activeBudgetId, timeZone, userUrl) = await GetUserDataAsync(cancellationToken);
        var text = await PrepareRelyAsync(
            activeBudgetId,
            timeZone,
            userUrl,
            cancellationToken);

        await SubmitReplyAsync(
            messageId,
            text,
            activeBudgetId.HasValue,
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
        menuTextBuilder.Append(
            string.Format(
                TR.L + "_MAIN_GREETING",
                userUrl));

        if (!budgetId.HasValue ||
            await GetBudgetNameAsync(budgetId.Value, cancellationToken) is not { } budgetName)
        {
            menuTextBuilder.Append(TR.L + "_MAIN_NO_BUDGET");
            return menuTextBuilder.ToString();
        }

        var transactions = await GetTransactionsReversedAsync(budgetId.Value, cancellationToken);
        
        menuTextBuilder.Append(
            string.Format(
                TR.L + "_MAIN_ACTIVE_BUDGET",
                budgetName,
                transactions.Sum(x => x.Amount)));

        if (transactions
                .Where(transaction => transaction.CreatedAt.Add(timeZone).Date == userToday)
                .OrderByDescending(transaction => transaction.CreatedAt)
                .ToArray() is not { } todayTransactions ||
            todayTransactions.Length == 0)
        {
            menuTextBuilder.Append(TR.L + "_MAIN_NO_TRANSACTIONS");
            return menuTextBuilder.ToString();
        }
        
        menuTextBuilder.Append(
            todayTransactions
                .CreatePage(
                    1024,
                    1,
                    (builder, _) =>
                    {
                        builder.Append(string.Format(TR.L + "_MAIN_TRANSACTION_INTRO", budgetName));
                    },
                    transaction => 
                        string.Format(
                            TR.L + (
                                transaction.Amount >= 0
                                ? "_MAIN_TRANSACTION_POSITIVE"
                                : "_MAIN_TRANSACTION_NEGATIVE"),
                            Math.Abs(transaction.Amount)) + 
                        string.Format(
                            TR.L + "_MAIN_TRANSACTION_COMMENT",
                            transaction.Comment?.EscapeHtml() ?? string.Empty),
                    (builder, currentString) => builder.Append(currentString),
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
        int callbackQueryMessageId,
        string reply,
        bool hasActiveBudget,
        CancellationToken cancellationToken)
    {
        await botWrapper.EditMessageTextAsync(
            currentUserService.TelegramUser.Id,
            text: reply,
            messageId: callbackQueryMessageId,
            parseMode: ParseMode.Html,
            replyMarkup: Keyboards.BuildMainInline(hasActiveBudget),
            cancellationToken: cancellationToken
        );
    }

    private async Task SubmitReplyAsync(
        string reply,
        bool hasActiveBudget,
        CancellationToken cancellationToken)
    {
        await botWrapper.SendTextMessageAsync(
            currentUserService.TelegramUser.Id,
            reply,
            parseMode: ParseMode.Html,
            replyMarkup: Keyboards.BuildMainInline(hasActiveBudget),
            cancellationToken: cancellationToken);
    }
}