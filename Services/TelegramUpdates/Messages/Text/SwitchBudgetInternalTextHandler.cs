using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Extensions;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class SwitchBudgetInternalTextHandler : ITextHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _db;

    public SwitchBudgetInternalTextHandler(
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
        return message.Text!.Trim().StartsWith("/switch_") &&
               Guid.TryParse(message.Text!.Trim()["/switch_".Length..], out _);
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var budgetId = Guid.Parse(message.Text!.Trim()["/switch_".Length..].Trim());

        var user = await _db.Users.SingleAsync(e => e.Id == _currentUserService.TelegramUser.Id, cancellationToken);
        if (await _db.Budgets.FirstOrDefaultAsync(e => e.Id == budgetId, cancellationToken) is not { } budget)
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
}