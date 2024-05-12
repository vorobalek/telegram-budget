using System.Text;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.DateTimeProvider;
using TelegramBudget.Services.TelegramBotClientWrapper;
using Tracee;

namespace TelegramBudget.Services.TelegramApi.NewHandlers;

public class NewMainHandler(
    ITelegramBotClientWrapper botWrapper,
    ICurrentUserService currentUserService,
    ApplicationDbContext db,
    IDateTimeProvider dateTime,
    ITracee tracee) : IBotCommandHandler, ICallbackQueryHandler
{
    public const string Command = "start";

    public async Task ProcessAsync(string _, CancellationToken cancellationToken)
    {
        using var trace = tracee.Scoped("main");
        var (activeBudgetId, timeZone, userUrl) = await GetUserDataAsync(trace, cancellationToken);

        var text = await PrepareRelyAsync(
            trace,
            activeBudgetId,
            timeZone,
            userUrl,
            cancellationToken);

        await SubmitReplyAsync(
            trace,
            text,
            activeBudgetId.HasValue,
            cancellationToken);
    }

    public async Task ProcessAsync(int messageId, string _, CancellationToken cancellationToken)
    {
        using var trace = tracee.Scoped("main");
        var (activeBudgetId, timeZone, userUrl) = await GetUserDataAsync(trace, cancellationToken);

        var text = await PrepareRelyAsync(
            trace,
            activeBudgetId,
            timeZone,
            userUrl,
            cancellationToken);

        await SubmitReplyAsync(
            trace,
            messageId,
            text,
            activeBudgetId.HasValue,
            cancellationToken);
    }

    private async Task<string> PrepareRelyAsync(
        ITracee trace,
        Guid? budgetId,
        TimeSpan timeZone,
        string userUrl,
        CancellationToken cancellationToken)
    {
        using var scope = trace.Scoped("prepare");
        var userToday = dateTime.UtcNow().DateTime.Add(timeZone).Date;

        var menuTextBuilder = new StringBuilder();
        menuTextBuilder.Append(
            string.Format(
                TR.L + "_MAIN_GREETING",
                userUrl));

        if (!budgetId.HasValue ||
            await GetBudgetNameAsync(trace, budgetId.Value, cancellationToken) is not { } budgetName)
        {
            menuTextBuilder.Append(TR.L + "_MAIN_NO_BUDGET");
            return menuTextBuilder.ToString();
        }

        var transactions = await GetTransactionsReversedAsync(trace, budgetId.Value, cancellationToken);

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
                    (builder, _) => { builder.Append(string.Format(TR.L + "_MAIN_TRANSACTION_INTRO", budgetName)); },
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
        ITracee trace,
        CancellationToken cancellationToken)
    {
        using var scope = trace.Scoped("get_user");
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

    private Task<string?> GetBudgetNameAsync(
        ITracee trace,
        Guid budgetId,
        CancellationToken cancellationToken)
    {
        using var scope = trace.Scoped("get_budget");
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
    > GetTransactionsReversedAsync(
        ITracee trace,
        Guid budgetId,
        CancellationToken cancellationToken)
    {
        using var scope = trace.Scoped("get_transactions");
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

    private Task<Message> SubmitReplyAsync(
        ITracee trace,
        int callbackQueryMessageId,
        string reply,
        bool hasActiveBudget,
        CancellationToken cancellationToken)
    {
        using var scope = trace.Scoped("submit");
        return botWrapper.EditMessageTextAsync(
            currentUserService.TelegramUser.Id,
            text: reply,
            messageId: callbackQueryMessageId,
            parseMode: ParseMode.Html,
            replyMarkup: Keyboards.BuildMainInline(hasActiveBudget),
            cancellationToken: cancellationToken
        );
    }

    private Task<Message> SubmitReplyAsync(
        ITracee trace,
        string reply,
        bool hasActiveBudget,
        CancellationToken cancellationToken)
    {
        using var scope = trace.Scoped("submit");
        return botWrapper.SendTextMessageAsync(
            currentUserService.TelegramUser.Id,
            reply,
            parseMode: ParseMode.Html,
            replyMarkup: Keyboards.BuildMainInline(hasActiveBudget),
            cancellationToken: cancellationToken);
    }
}