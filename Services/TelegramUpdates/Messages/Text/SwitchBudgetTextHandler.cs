using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class SwitchBudgetTextHandler : ITextHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _db;

    public SwitchBudgetTextHandler(
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
        return message.Text!.Trim().StartsWith("/switch") &&
               !message.Text!.Trim().StartsWith("/switch_");
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var budgetName = await ExtractBudgetNameAsync(message, cancellationToken);

        if (budgetName is null) return;

        var user = await _db.Users.SingleAsync(e => e.Id == _currentUserService.TelegramUser.Id, cancellationToken);
        if (await GetBudgetAsync(budgetName, cancellationToken) is not { } budget)
            return;

        user.ActiveBudgetId = budget.Id;
        _db.Users.Update(user);
        await _db.SaveChangesAsync(cancellationToken);

        await _bot
            .SendTextMessageAsync(
                _currentUserService.TelegramUser.Id,
                $"✅ Вашим активным бюджетом выбран &quot;{budget.Name.EscapeHtml()}&quot; " +
                (budget.Owner is not { } owner
                    ? "<i>(владелец неизвестен)</i> "
                    : owner.Id == _currentUserService.TelegramUser.Id
                        ? "<i>(владелец – вы)</i> "
                        : $"<i>(владелец – {budget.Owner.GetFullNameLink()})</i> "),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }

    private async Task<string?> ExtractBudgetNameAsync(Message message, CancellationToken cancellationToken)
    {
        var budgetName = message.Text!.Trim()["/switch".Length..].Trim();
        if (!string.IsNullOrWhiteSpace(budgetName)) return budgetName;

        await _bot
            .SendTextMessageAsync(
                _currentUserService.TelegramUser.Id,
                "/switch &lt;название бюджета&gt; - Переключить активный бюджет. Список доступных бюджетов можно узнать командой /list.",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        return null;
    }

    private async Task<Budget?> GetBudgetAsync(
        string budgetName,
        CancellationToken cancellationToken)
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
                        "<i>Выберите тот, на который хотите переключиться и кликните на соответствующую ему команду, она отправится боту.</i>");
                    pageBuilder.AppendLine();
                },
                budget =>
                {
                    return $"{budget.Name.EscapeHtml()} " +
                           (budget.Owner is not { } owner
                               ? "<i>(владелец неизвестен)</i> "
                               : owner.Id == _currentUserService.TelegramUser.Id
                                   ? "<i>(владелец – вы)</i> "
                                   : $"<i>(владелец – {budget.Owner.GetFullNameLink()})</i> ") +
                           " ➡️ " +
                           $"/switch_{budget.Id:N}";
                },
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
}