using System.Text;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.DateTimeProvider;
using TelegramBudget.Services.TelegramBotClientWrapper;
using Tracee;

namespace TelegramBudget.Services.TelegramApi.NewFlow;

internal sealed class NewMain(
    ITracee tracee,
    ITelegramBotWrapper botWrapper,
    ICurrentUserService currentUserService,
    ApplicationDbContext db,
    IDateTimeProvider dateTime) : IBotCommandFlow, ICallbackQueryFlow
{
    public const string Command = "start";

    public async Task ProcessAsync(string __, CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("main");
        
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

    public async Task ProcessAsync(int messageId, string __, CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("main");
        
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
        using var __ = tracee.Scoped("prepare");
        
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
        CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("get_user");
        
        var data = await db.User
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

    private async Task<string?> GetBudgetNameAsync(
        Guid budgetId,
        CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("get_budget");
        
        return await db.Budget
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
        Guid budgetId,
        CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("get_transactions");
        
        var data = await db.Transaction
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
        using var _ = tracee.Scoped("submit");
        
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
        using var _ = tracee.Scoped("submit");
        
        await botWrapper.SendTextMessageAsync(
            currentUserService.TelegramUser.Id,
            reply,
            parseMode: ParseMode.Html,
            replyMarkup: Keyboards.BuildMainInline(hasActiveBudget),
            cancellationToken: cancellationToken);
    }
}