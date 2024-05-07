using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBudget.Configuration;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using User = TelegramBudget.Data.Entities.User;

namespace TelegramBudget.Services.TelegramApi.NewHandlers;

public class NewHistoryBotCommand(
    ITelegramBotClient bot,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
{
    public async Task ProcessAsync(
        string data,
        Message? callbackQueryMessage,
        CancellationToken cancellationToken)
    {
        if (callbackQueryMessage is null) return;

        var user = await GetUserAsync(cancellationToken);

        var (budgetId, pageNumber) = ParseArguments(data);

        if (await GetBudgetAsync(budgetId, user, cancellationToken) is not { } budget)
        {
            await SubmitAsync(
                TR.L + "NO_BUDGET",
                callbackQueryMessage.MessageId,
                cancellationToken);
            return;
        }

        if (budget.Transactions.Count == 0)
        {
            await SubmitAsync(
                string.Format(TR.L + "NO_TRANSACTIONS", budget.Name.EscapeHtml()),
                callbackQueryMessage.MessageId,
                cancellationToken);
            return;
        }

        if (GetPageContent(
                pageNumber,
                budget, 
                budget.Transactions, 
                user, 
                out var actualPageNumber,
                out var actualPageCount) is not { } pageContent)
        {
            await SubmitAsync(
                string.Format(TR.L + "NO_HISTORY_PAGE", pageNumber),
                callbackQueryMessage.MessageId,
                cancellationToken);
            return;
        }

        await SubmitAsync(
            pageContent,
            callbackQueryMessage.MessageId,
            cancellationToken,
            $"{budget.Id:N}",
            actualPageNumber,
            actualPageCount);
    }

    private Task<User> GetUserAsync(CancellationToken cancellationToken)
    {
        return db.Users.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);
    }

    private (Guid? budgetId, int pageNumber) ParseArguments(string data)
    {
        var arguments = data.Split('.').Skip(1).ToArray();
        
        if (arguments.Length < 2) return (null, 1);
        
        Guid? budgetId = Guid.TryParse(arguments[0], out var budgetGuid) ? budgetGuid : null;
        var pageNumber = int.Parse(arguments[1]);
        
        return (budgetId, pageNumber);
    }

    private Task<Budget?> GetBudgetAsync(Guid? budgetId, User user, CancellationToken cancellationToken)
    {
        return budgetId is not null 
            ? db.Budgets.SingleOrDefaultAsync(e => e.Id == budgetId, cancellationToken) 
            : Task.FromResult(user.ActiveBudget);
    }

    private string? GetPageContent(
        int requestedPageNumber,
        Budget budget, 
        ICollection<Transaction> budgetTransactions,
        User user,
        out int actualPageNumber,
        out int actualPageCount)
    {
        var currentSum = 0m;
        var reversedTransactions = budgetTransactions
            .Select(transaction =>
            {
                var transactionModel = new
                {
                    BeforeSum = currentSum,
                    AfterSum = currentSum + transaction.Amount,
                    transaction.Amount,
                    transaction.Comment,
                    transaction.Author,
                    transaction.CreatedAt
                };
                currentSum += transaction.Amount;
                return transactionModel;
            })
            .OrderByDescending(x => x.CreatedAt);

        var pageContent = reversedTransactions
            .CreatePage(
                768,
                requestedPageNumber,
                (pageBuilder, pageNumber) =>
                    pageBuilder.AppendLine(
                        string.Format(TR.L + "HISTORY_INTRO", budget.Name.EscapeHtml(), pageNumber)),
                transaction =>
                    $"{transaction.BeforeSum:0.00} " +
                    $"<b>{(transaction.Amount >= 0 ? "➕ " + transaction.Amount.ToString("0.00") : "➖ " + Math.Abs(transaction.Amount).ToString("0.00"))}</b> " +
                    $"➡️ {transaction.AfterSum:0.00}" +
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
                        user.TimeZone == TimeSpan.Zero
                            ? TR.L + transaction.CreatedAt + AppConfiguration.DateTimeFormat + " UTC"
                            : TR.L + transaction.CreatedAt.Add(user.TimeZone) + AppConfiguration.DateTimeFormat,
                        transaction.Author.GetFullNameLink()) +
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

    private Task SubmitAsync(
        string text,
        int messageId,
        CancellationToken cancellationToken,
        string? budgetSlug = null,
        int actualPageNumber = 0, 
        int actualPageCount = 0)
    {
        var keyboard = GetKeyboard(
            budgetSlug,
            actualPageNumber, 
            actualPageCount);
        
        return bot
            .EditMessageTextAsync(
                currentUserService.TelegramUser.Id,
                messageId,
                text,
                parseMode: ParseMode.Html,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
    }

    private InlineKeyboardMarkup GetKeyboard(
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