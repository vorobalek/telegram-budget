using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class CreateBudgetTextHandler : ITextHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _db;

    public CreateBudgetTextHandler(
        ITelegramBotClient bot,
        ICurrentUserService currentUserService, ApplicationDbContext db)
    {
        _bot = bot;
        _currentUserService = currentUserService;
        _db = db;
    }

    public bool ShouldBeInvoked(Message message)
    {
        return message.Text!.Trim().StartsWith("/create");
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var budgetName = await ExtractBudgetNameAsync(message, cancellationToken);

        if (budgetName is null) return;

        if (await BudgetAlreadyExistsAsync(budgetName, cancellationToken))
            return;

        var user = await _db.Users.SingleAsync(e => e.Id == _currentUserService.TelegramUser.Id, cancellationToken);
        var newBudget = new Budget
        {
            Name = budgetName,
            ActiveUsers = new[]
            {
                user
            }
        };
        await _db.Budgets.AddAsync(newBudget, cancellationToken);

        var participating = new Participating
        {
            Budget = newBudget,
            Participant = user
        };
        await _db.Participating.AddAsync(participating, cancellationToken);

        user.ActiveBudget = newBudget;
        _db.Update(user);

        await _db.SaveChangesAsync(cancellationToken);

        await _bot
            .SendTextMessageAsync(
                _currentUserService.TelegramUser.Id,
                $"✅ Создан новый бюджет с именем &quot;{budgetName.EscapeHtml()}&quot;, установлен как активный.",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }

    private async Task<string?> ExtractBudgetNameAsync(Message message, CancellationToken cancellationToken)
    {
        var budgetName = message.Text!.Trim()["/create".Length..].Trim().Truncate(250);
        if (!string.IsNullOrWhiteSpace(budgetName)) return budgetName;

        await _bot
            .SendTextMessageAsync(
                _currentUserService.TelegramUser.Id,
                "/create &lt;название бюджета&gt; - Создать ноый бюджет. Новый бюджет автоматически станет активным.",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        return null;
    }

    private async Task<bool> BudgetAlreadyExistsAsync(string budgetName, CancellationToken cancellationToken)
    {
        var budgetAlreadyExists = await _db.Budgets.AnyAsync(e => e.Name == budgetName, cancellationToken);
        if (!budgetAlreadyExists) return false;

        await _bot
            .SendTextMessageAsync(
                _currentUserService.TelegramUser.Id,
                $"❌ Бюджет с именем &quot;{budgetName.EscapeHtml()}&quot; уже существует.",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        return true;
    }
}