using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBudget.Configuration;
using TelegramBudget.Data;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.NewHandlers;

public class NewHistoryBotCommand(
    ITelegramBotClientWrapper botWrapper,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
{
    public async Task ProcessAsync(
        string data,
        int? callbackQueryMessageId,
        CancellationToken cancellationToken)
    {
        if (!callbackQueryMessageId.HasValue) return;

        var (budgetId, pageNumber) = ParseArguments(data);
        var (activeBudgetId, timeZone) = await GetUserDataAsync(cancellationToken);

        var reply = await PrepareReplyAsync(
            budgetId ?? activeBudgetId,
            pageNumber,
            timeZone,
            cancellationToken);

        await SubmitReplyAsync(
            callbackQueryMessageId.Value,
            reply,
            cancellationToken);
    }

    private static (Guid? budgetId, int pageNumber) ParseArguments(string data)
    {
        var arguments = data.Split('.').Skip(1).ToArray();

        if (arguments.Length < 2) return (null, 1);

        Guid? budgetId = Guid.TryParse(arguments[0], out var budgetGuid) ? budgetGuid : null;
        var pageNumber = int.Parse(arguments[1]);

        return (budgetId, pageNumber);
    }

    private async Task<(Guid? ActiveBudgetId, TimeSpan TimeZone)> GetUserDataAsync(CancellationToken cancellationToken)
    {
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
        Guid? budgetId,
        int pageNumber,
        TimeSpan timeZone,
        CancellationToken cancellationToken)
    {
        if (!budgetId.HasValue ||
            await GetBudgetNameAsync(budgetId.Value, cancellationToken) is not { } budgetName)
            return (TR.L + "NO_BUDGET", null, 0, 0);

        var transactions = await GetTransactionsReversedAsync(budgetId.Value, cancellationToken);
        if (transactions.Count == 0)
            return (string.Format(TR.L + "NO_TRANSACTIONS", budgetName.EscapeHtml()), null, 0, 0);

        var pageContent = BuildPageContent(
            pageNumber,
            budgetName,
            transactions,
            timeZone,
            out var actualPageNumber,
            out var actualPageCount);
        if (string.IsNullOrWhiteSpace(pageContent))
            return (string.Format(TR.L + "NO_PAGE_CONTENT", pageNumber), null, 0, 0);

        return (pageContent, $"{budgetId:N}", actualPageNumber, actualPageCount);
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
            decimal BeforeSum,
            decimal Amount,
            string? Comment,
            DateTime CreatedAt,
            string AuthorUrl)>
    > GetTransactionsReversedAsync(Guid budgetId, CancellationToken cancellationToken)
    {
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
                    TelegramHelper.GetFullNameLink(e.AuthorId, e.AuthorFirstName, e.AuthorLastName));
                currentSum += e.Amount;
                return transaction;
            })
            .OrderByDescending(e => e.CreatedAt)
            .ToArray();
    }

    private static string? BuildPageContent(
        int requestedPageNumber,
        string budgetName,
        IEnumerable<(decimal BeforeSum, decimal Amount, string? Comment, DateTime CreatedAt, string AuthorUrl)>
            transactions,
        TimeSpan timeZone,
        out int actualPageNumber,
        out int actualPageCount)
    {
        var pageContent = transactions
            .CreatePage(
                768,
                requestedPageNumber,
                (pageBuilder, pageNumber) =>
                    pageBuilder.AppendLine(
                        string.Format(TR.L + "HISTORY_INTRO", budgetName.EscapeHtml(), pageNumber)),
                transaction =>
                    $"{transaction.BeforeSum:0.00} " +
                    $"<b>{(transaction.Amount >= 0
                        ? $"➕ {transaction.Amount:0.00}"
                        : $"➖ {Math.Abs(transaction.Amount):0.00}")}</b> " +
                    $"➡️ {transaction.BeforeSum + transaction.Amount:0.00}" +
                    (transaction.Comment is not null
                        ? Environment.NewLine +
                          Environment.NewLine +
                          transaction.Comment.EscapeHtml()
                        : string.Empty) +
                    Environment.NewLine +
                    Environment.NewLine +
                    "<i>" +
                    string.Format(
                        TR.L + "ADDED_NOTICE",
                        timeZone == TimeSpan.Zero
                            ? TR.L + transaction.CreatedAt + AppConfiguration.DateTimeFormat + " UTC"
                            : TR.L + transaction.CreatedAt.Add(timeZone) + AppConfiguration.DateTimeFormat,
                        transaction.AuthorUrl) +
                    "</i>",
                (pageBuilder, currentString) =>
                {
                    pageBuilder.AppendLine();
                    pageBuilder.AppendLine();
                    pageBuilder.AppendLine(currentString);
                },
                out actualPageNumber,
                out actualPageCount);

        return pageContent;
    }

    private Task<Message> SubmitReplyAsync(
        int messageId,
        (string Text, string? BudgetSlug, int PageNumber, int PageCount) reply,
        CancellationToken cancellationToken)
    {
        var keyboard = GetKeyboard(
            reply.BudgetSlug,
            reply.PageNumber,
            reply.PageCount);

        return botWrapper
            .EditMessageTextAsync(
                currentUserService.TelegramUser.Id,
                messageId,
                reply.Text,
                ParseMode.Html,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
    }

    private static InlineKeyboardMarkup GetKeyboard(
        string? budgetSlug,
        int actualPageNumber,
        int actualPageCount)
    {
        return Keyboards.GetPaginationInline(
            actualPageNumber,
            actualPageCount,
            (_, targetPageNumber) =>
                $"hst.{budgetSlug ?? throw new ArgumentNullException(nameof(budgetSlug))}.{targetPageNumber}",
            (_, targetPageNumber) =>
                $"hst.{budgetSlug ?? throw new ArgumentNullException(nameof(budgetSlug))}.{targetPageNumber}");
    }
}