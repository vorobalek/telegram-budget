using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Extensions;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class UnShareBudgetInternalTextHandler : ITextHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _db;

    public UnShareBudgetInternalTextHandler(
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
        return message.Text!.Trim().StartsWith("/unshare_") &&
               message.Text!.Trim()["/unshare_".Length..].Split('_') is { } args &&
               args.Length == 2 &&
               long.TryParse(args[0], out _) &&
               Guid.TryParse(args[1], out _);
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var args = message.Text!.Trim()["/unshare_".Length..].Split('_');
        var budgetId = Guid.Parse(args[1]);

        if (await _db
                .Budgets
                .FirstOrDefaultAsync(e => e.Id == budgetId, cancellationToken) is not { } budgetToUnShare)
            return;

        var userToShareId = long.Parse(args[0]);
        var userToUnShare = await _db
            .Users
            .SingleAsync(e => e.Id == userToShareId, cancellationToken);

        if (await _db
                .Participating
                .FirstOrDefaultAsync(e =>
                        e.ParticipantId == userToUnShare.Id &&
                        e.BudgetId == budgetToUnShare.Id,
                    cancellationToken) is not { } participant)
        {
            await _bot
                .SendTextMessageAsync(
                    _currentUserService.TelegramUser.Id,
                    $"❌ Пользователь {userToUnShare.GetFullNameLink()} не имеет доступа к бюджету с именем &quot;{budgetToUnShare.Name.EscapeHtml()}&quot;",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        if (budgetToUnShare.CreatedBy == userToUnShare.Id)
        {
            await _bot
                .SendTextMessageAsync(
                    _currentUserService.TelegramUser.Id,
                    $"❌ Пользователь {userToUnShare.GetFullNameLink()} не может быть лишен доступа к бюджету с именем &quot;{budgetToUnShare.Name.EscapeHtml()}&quot;, поскольку является его владельцем",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        _db.Participating.Remove(participant);

        budgetToUnShare.Participating.Remove(participant);
        _db.Budgets.Update(budgetToUnShare);

        userToUnShare.ActiveBudget ??= budgetToUnShare;
        _db.Users.Update(userToUnShare);

        await _db.SaveChangesAsync(cancellationToken);

        var participantIds = await _db
            .Participating
            .Where(e => e.BudgetId == budgetToUnShare.Id)
            .Select(e => e.ParticipantId)
            .ToListAsync(cancellationToken);

        foreach (var participantId in participantIds)
            await _bot
                .SendTextMessageAsync(
                    participantId,
                    $"✅ Доступ к бюджету с именем &quot;{budgetToUnShare.Name.EscapeHtml()}&quot; отозван у {userToUnShare.GetFullNameLink()}" +
                    Environment.NewLine +
                    Environment.NewLine +
                    $"<i>Инициатор: {_currentUserService.TelegramUser.GetFullNameLink()}</i>",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);

        await _bot
            .SendTextMessageAsync(
                userToUnShare.Id,
                $"❗ У вас отозван доступ к бюдету &quot;{budgetToUnShare.Name.EscapeHtml()}&quot; пользователем {_currentUserService.TelegramUser.GetFullNameLink()}",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }
}