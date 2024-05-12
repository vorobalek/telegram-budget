using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBudget.Configuration;
using TelegramBudget.Data;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;
using Tracee;

namespace TelegramBudget.Services.TelegramApi.NewHandlers;

public class NewHistoryHandler(
    ITelegramBotClientWrapper botWrapper,
    ICurrentUserService currentUserService,
    ApplicationDbContext db,
    ITracee tracee) : ICallbackQueryHandler
{
    public const string Command = "history";
    public const string CommandPrefix = "history.";

    public async Task ProcessAsync(
        int messageId,
        string data,
        CancellationToken cancellationToken)
    {
        using var trace = tracee.Scoped("history");
        var (budgetId, requestedPageNumber) = ParseArguments(trace, data);
        var (activeBudgetId, timeZone) = await GetUserDataAsync(trace, cancellationToken);

        var (text, budgetSlug, pageNumber, pageCount) = await PrepareReplyAsync(
            trace,
            budgetId ?? activeBudgetId,
            requestedPageNumber,
            timeZone,
            cancellationToken);

        await SubmitReplyAsync(
            trace,
            messageId,
            text,
            budgetSlug,
            pageNumber,
            pageCount,
            cancellationToken);
    }

    private static (Guid? budgetId, int pageNumber) ParseArguments(
        ITracee trace,
        string data)
    {
        using var scope = trace.Scoped("parse");
        var arguments = data.Split('.').Skip(1).ToArray();

        if (arguments.Length < 2) return (null, 1);

        Guid? budgetId = Guid.TryParse(arguments[0], out var budgetGuid) ? budgetGuid : null;
        var pageNumber = int.Parse(arguments[1]);

        return (budgetId, pageNumber);
    }

    private async Task<(Guid? ActiveBudgetId, TimeSpan TimeZone)> GetUserDataAsync(
        ITracee trace,
        CancellationToken cancellationToken)
    {
        using var scope = trace.Scoped("get_user");
        var data = await db.Users
            .Where(e => e.Id == currentUserService.TelegramUser.Id)
            .Select(e => new
            {
                e.ActiveBudgetId,
                e.TimeZone
            })
            .SingleAsync(cancellationToken);

        return (data.ActiveBudgetId, data.TimeZone);
    }

    private async Task<(string Text, string? BudgetSlug, int PageNumber, int PageCount)> PrepareReplyAsync(
        ITracee trace,
        Guid? budgetId,
        int pageNumber,
        TimeSpan timeZone,
        CancellationToken cancellationToken)
    {
        using var scope = trace.Scoped("prepare");
        if (!budgetId.HasValue ||
            await GetBudgetNameAsync(scope, budgetId.Value, cancellationToken) is not { } budgetName)
            return (TR.L + "_HISTORY_NO_BUDGET", null, 0, 0);

        var transactions = await GetTransactionsReversedAsync(
            scope,
            budgetId.Value,
            cancellationToken);
        if (transactions.Count == 0)
            return (string.Format(TR.L + "_HISTORY_NO_TRANSACTIONS", budgetName.EscapeHtml()), null, 0, 0);

        var pageContent = BuildPageContent(
            scope,
            pageNumber,
            budgetName,
            transactions,
            timeZone,
            out var actualPageNumber,
            out var actualPageCount);
        if (string.IsNullOrWhiteSpace(pageContent))
            return (string.Format(TR.L + "_HISTORY_NO_CONTENT", pageNumber), null, 0, 0);

        return (pageContent, $"{budgetId:N}", actualPageNumber, actualPageCount);
    }

    private async Task<string?> GetBudgetNameAsync(
        ITracee trace,
        Guid budgetId,
        CancellationToken cancellationToken)
    {
        using var scope = trace.Scoped("get_budget");
        return await db.Budgets
            .Where(e => e.Id == budgetId)
            .Select(e => e.Name)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task<
        ICollection<(
            decimal BeforeSum,
            decimal Amount,
            string? Comment,
            DateTime CreatedAt,
            long AuthorId,
            string AuthorName)>
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
                e.CreatedAt,
                AuthorId = e.Author.Id,
                AuthorFirstName = e.Author.FirstName,
                AuthorLastName = e.Author.LastName
            })
            .ToArrayAsync(cancellationToken);

        var currentSum = 0m;
        return data.Select(e =>
            {
                var transaction = (
                    currentSum,
                    e.Amount,
                    e.Comment,
                    e.CreatedAt,
                    e.AuthorId,
                    TelegramHelper.GetFullName(e.AuthorFirstName, e.AuthorLastName));
                currentSum += e.Amount;
                return transaction;
            })
            .OrderByDescending(e => e.CreatedAt)
            .ToArray();
    }

    private static string? BuildPageContent(
        ITracee trace,
        int requestedPageNumber,
        string budgetName,
        IEnumerable<(
            decimal BeforeSum,
            decimal Amount,
            string? Comment,
            DateTime CreatedAt,
            long AuthorId,
            string AuthorName)> transactions,
        TimeSpan timeZone,
        out int actualPageNumber,
        out int actualPageCount)
    {
        using var scope = trace.Scoped("build_page");
        var pageContent = transactions
            .CreatePage(
                768,
                requestedPageNumber,
                (pageBuilder, pageNumber) => pageBuilder.Append(
                    string.Format(
                            TR.L + "_HISTORY_INTRO",
                            budgetName.EscapeHtml(),
                            pageNumber)
                        .WithFallbackValue()),
                transaction =>
                {
                    var diff = string.Format(
                        TR.L + (
                            transaction.Amount >= 0
                                ? "_HISTORY_TRANSACTION_POSITIVE"
                                : "_HISTORY_TRANSACTION_NEGATIVE"),
                        transaction.BeforeSum,
                        Math.Abs(transaction.Amount),
                        transaction.BeforeSum + transaction.Amount);

                    var comment = string.IsNullOrWhiteSpace(transaction.Comment)
                        ? string.Empty
                        : string.Format(
                            TR.L + "_HISTORY_TRANSACTION_COMMENT",
                            transaction.Comment?.EscapeHtml() ?? string.Empty);

                    var author = string.Format(
                        TR.L + "_HISTORY_TRANSACTION_ADDED_BY",
                        timeZone == TimeSpan.Zero
                            ? TR.L + transaction.CreatedAt + AppConfiguration.DateTimeFormat + " UTC"
                            : TR.L + transaction.CreatedAt.Add(timeZone) + AppConfiguration.DateTimeFormat,
                        transaction.AuthorId,
                        transaction.AuthorName);

                    return $"{diff}{comment}{author}";
                },
                (pageBuilder, currentString) => pageBuilder.Append(currentString),
                out actualPageNumber,
                out actualPageCount);

        return pageContent;
    }

    private async Task SubmitReplyAsync(ITracee trace,
        int messageId,
        string text,
        string? budgetSlug,
        int pageNumber,
        int pageCount,
        CancellationToken cancellationToken)
    {
        using var scope = trace.Scoped("submit");
        var keyboard = GetKeyboard(
            scope,
            budgetSlug,
            pageNumber,
            pageCount);

        await botWrapper
            .EditMessageTextAsync(
                currentUserService.TelegramUser.Id,
                messageId,
                text,
                ParseMode.Html,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
    }

    private static InlineKeyboardMarkup GetKeyboard(
        ITracee trace,
        string? budgetSlug,
        int actualPageNumber,
        int actualPageCount)
    {
        using var scope = trace.Scoped("get_keyboard");
        return new InlineKeyboardMarkup(
            Keyboards.BuildPaginationInlineButtons(
                    actualPageNumber,
                    actualPageCount,
                    (_, targetPageNumber) =>
                        $"{CommandPrefix}{budgetSlug ?? throw new ArgumentNullException(nameof(budgetSlug))}.{targetPageNumber}",
                    (_, targetPageNumber) =>
                        $"{CommandPrefix}{budgetSlug ?? throw new ArgumentNullException(nameof(budgetSlug))}.{targetPageNumber}")
                .Concat([[Keyboards.BackToMainInlineButton]]));
    }
}