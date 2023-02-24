using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;
using User = TelegramBudget.Data.Entities.User;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class HistoryTextHandler : ITextHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _db;

    public HistoryTextHandler(
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
        return message.Text!.Trim().StartsWith("/history") &&
               !message.Text!.Trim().StartsWith("/history_");
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var user = await _db.Users.SingleAsync(e => e.Id == _currentUserService.TelegramUser.Id, cancellationToken);
        var budgetName = message.Text!.Trim()["/history".Length..].Trim();

        if (await ExtractBudgetAsync(budgetName, user, cancellationToken) is not { } budget)
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

    private async Task<Budget?> ExtractBudgetAsync(string budgetName, User user, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(budgetName))
        {
            if (await _db
                    .Budgets
                    .Where(e => e.Name == budgetName)
                    .ToListAsync(cancellationToken) is { Count: > 0 } budgets)
            {
                if (budgets.Count == 1) return budgets[0];

                await budgets.SendPaginatedAsync(
                    (pageBuilder, pageNumber) =>
                    {
                        pageBuilder.AppendLine(
                            $"❌ <b>Доступно несколько бюджетов с именем &quot;{budgetName.EscapeHtml()}&quot;</b> <i>(страница {pageNumber})</i>");
                        pageBuilder.AppendLine();
                        pageBuilder.AppendLine(
                            "<i>Выберите тот, для которого хотите получить историю и кликните на соответствующую ему команду, она отправится боту.</i>");
                        pageBuilder.AppendLine();
                    },
                    budget => $"{budget.Name.EscapeHtml()} " +
                              (budget.Owner is not { } owner
                                  ? "<i>(владелец неизвестен)</i> "
                                  : owner.Id == _currentUserService.TelegramUser.Id
                                      ? "<i>(владелец – вы)</i> "
                                      : $"<i>(владелец – {budget.Owner.GetFullNameLink()})</i> ") +
                              " ➡️ " +
                              $"/history_{budget.Id:N}",
                    (pageBuilder, currentString) =>
                    {
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
                return null;
            }

            await _bot
                .SendTextMessageAsync(
                    _currentUserService.TelegramUser.Id,
                    $"❌ Не найден бюджет с именем &quot;{budgetName.EscapeHtml()}&quot;",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return null;
        }

        if (user.ActiveBudget is { } activeBudget)
            return activeBudget;

        await _bot
            .SendTextMessageAsync(
                _currentUserService.TelegramUser.Id,
                "❌ У вас не выбран активный бюджет. Установите его командой /switch",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        return null;
    }
}