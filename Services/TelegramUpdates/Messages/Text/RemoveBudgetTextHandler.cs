using System.Text;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;
using User = TelegramBudget.Data.Entities.User;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class RemoveBudgetTextHandler : ITextHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _db;

    public RemoveBudgetTextHandler(
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
        return message.Text!.Trim().StartsWith("/remove");
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var user = await _db.Users.SingleAsync(e => e.Id == _currentUserService.TelegramUser.Id, cancellationToken);
        if (await ExtractBudgetAsync(message, user, cancellationToken) is not { } budget)
            return;

        if (budget.CreatedBy != _currentUserService.TelegramUser.Id)
        {
            await _bot
                .SendTextMessageAsync(
                    _currentUserService.TelegramUser.Id,
                    $"❌ Вы не можете удалить бюджет с именем &quot;{budget.Name.EscapeHtml()}&quot;, " +
                    "поскольку не являетесь его владельцем.",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        var participants = await _db
            .Participating
            .Where(e => e.BudgetId == budget.Id)
            .ToListAsync(cancellationToken);

        _db.Budgets.Remove(budget);
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var participant in participants)
            await _bot
                .SendTextMessageAsync(
                    participant.ParticipantId,
                    $"❗ Бюджет с именем &quot;{budget.Name.EscapeHtml()}&quot; безвозвратно удален вместе с транзакциями." +
                    Environment.NewLine +
                    Environment.NewLine +
                    $"<i>Инициатор: {_currentUserService.TelegramUser.GetFullNameLink()}</i>",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
    }

    private async Task<Budget?> ExtractBudgetAsync(Message message, User user, CancellationToken cancellationToken)
    {
        var errorMessageBuilder = new StringBuilder();
        errorMessageBuilder.AppendLine("/remove (&lt;название бюджета&gt;) - Удалить бюджет. " +
                                       "Все транзакции будут безвозвратно удалены. " +
                                       "Название бюджета не обязательно. " +
                                       "Без указания названия бюджета будет выбран активный бюджет.");
        errorMessageBuilder.AppendLine();

        var budgetName = message.Text!.Trim()["/remove".Length..].Trim();
        if (!string.IsNullOrWhiteSpace(budgetName))
        {
            if (await _db.Budgets.FirstOrDefaultAsync(e => e.Name == budgetName, cancellationToken) is { } budget)
                return budget;

            errorMessageBuilder.AppendLine($"❌ Не найден бюджет с именем &quot;{budgetName.EscapeHtml()}&quot;.");
        }
        else
        {
            if (user.ActiveBudget is { } activeBudget)
                return activeBudget;

            errorMessageBuilder.AppendLine("❌ У вас не выбран активный бюджет. Установите его командой /switch");
        }

        await _bot
            .SendTextMessageAsync(
                _currentUserService.TelegramUser.Id,
                errorMessageBuilder.ToString(),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        return null;
    }
}