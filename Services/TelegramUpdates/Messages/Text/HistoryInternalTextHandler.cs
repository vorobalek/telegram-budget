using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Extensions;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class HistoryInternalTextHandler : ITextHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _db;

    public HistoryInternalTextHandler(
        ITelegramBotClient bot,
        ICurrentUserService currentUserService,
        ApplicationDbContext db)
    {
        _bot = bot;
        _currentUserService = currentUserService;
        _db = db;
    }

    public bool ShouldBeInvoked(Message message)
    {
        return message.Text!.Trim().StartsWith("/history_") &&
               Guid.TryParse(message.Text!.Trim()["/history_".Length..], out _);
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var budgetId = Guid.Parse(message.Text!.Trim()["/history_".Length..].Trim());

        var user = await _db.Users.SingleAsync(e => e.Id == _currentUserService.TelegramUser.Id, cancellationToken);
        if (await _db.Budgets.FirstOrDefaultAsync(e => e.Id == budgetId, cancellationToken) is not { } budget)
            return;

        if (!budget.Transactions.Any())
        {
            await _bot
                .SendTextMessageAsync(
                    _currentUserService.TelegramUser.Id,
                    $"❌ У вас пока нет транзакций в бюджете с именем &quot;{budget.Name}&quot;. Начните добавлять транзакции: например,<b> -100 за кофе</b>",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        var currentAmount = 0m;
        await budget.Transactions.OrderBy(e => e.CreatedAt).SendPaginatedAsync(
            (pageBuilder, pageNumber) =>
                pageBuilder.AppendLine(
                    $"✅ <b>История транзакций по бюджету: &quot;{budget.Name}&quot;</b> <i>(страница {pageNumber})</i>"),
            transaction =>
            {
                var currentString =
                    $"{currentAmount:0.00} " +
                    $"<b>{(transaction.Amount >= 0 ? "➕ " + transaction.Amount.ToString("0.00") : "➖ " + Math.Abs(transaction.Amount).ToString("0.00"))}</b> " +
                    $"➡️ {currentAmount + transaction.Amount:0.00}" +
                    (transaction.Comment is not null
                        ? Environment.NewLine +
                          Environment.NewLine +
                          transaction.Comment.EscapeHtml()
                        : string.Empty) +
                    Environment.NewLine +
                    Environment.NewLine +
                    $"<i>добавлено {(user.TimeZone == TimeSpan.Zero ? transaction.CreatedAt.ToString("dd.MM.yyyy HH:mm") + " UTC" : transaction.CreatedAt.Add(user.TimeZone).ToString("dd.MM.yyyy HH:mm"))} " +
                    $"{transaction.Author.GetFullNameLink()}</i>";
                currentAmount += transaction.Amount;
                return currentString;
            },
            (pageBuilder, currentString) =>
            {
                pageBuilder.AppendLine();
                pageBuilder.AppendLine();
                pageBuilder.AppendLine(currentString);
            },
            async (pageContent, token) =>
                await _bot
                    .SendTextMessageAsync(
                        _currentUserService.TelegramUser.Id,
                        pageContent,
                        parseMode: ParseMode.Html,
                        cancellationToken: token),
            4096,
            cancellationToken);
    }
}