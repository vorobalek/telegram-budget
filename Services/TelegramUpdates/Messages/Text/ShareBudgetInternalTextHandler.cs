using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class ShareBudgetInternalTextHandler : ITextHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _db;

    public ShareBudgetInternalTextHandler(
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
        return message.Text!.Trim().StartsWith("/share_") &&
               message.Text!.Trim()["/share_".Length..].Split('_') is { } args &&
               args.Length == 2 &&
               long.TryParse(args[0], out _) &&
               Guid.TryParse(args[1], out _);
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var args = message.Text!.Trim()["/share_".Length..].Split('_');
        var budgetId = Guid.Parse(args[1]);

        if (await _db
                .Budgets
                .FirstOrDefaultAsync(e => e.Id == budgetId, cancellationToken) is not { } budgetToShare)
            return;

        var userToShareId = long.Parse(args[0]);
        var userToShare = await _db
            .Users
            .SingleAsync(e => e.Id == userToShareId, cancellationToken);

        if (await _db
                .Participating
                .AnyAsync(e =>
                        e.ParticipantId == userToShareId &&
                        e.BudgetId == budgetId,
                    cancellationToken))
        {
            await _bot
                .SendTextMessageAsync(
                    _currentUserService.TelegramUser.Id,
                    $"❌ Пользователь {userToShare.GetFullNameLink()} уже имеет доступ к бюджету с именем &quot;{budgetToShare.Name.EscapeHtml()}&quot;",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        var participantIds = await _db
            .Participating
            .Where(e => e.BudgetId == budgetToShare.Id)
            .Select(e => e.ParticipantId)
            .ToListAsync(cancellationToken);

        var newParticipant = new Participating
        {
            Participant = userToShare,
            Budget = budgetToShare
        };
        await _db.Participating.AddAsync(newParticipant, cancellationToken);

        budgetToShare.Participating.Add(newParticipant);
        _db.Budgets.Update(budgetToShare);

        userToShare.ActiveBudget ??= budgetToShare;
        _db.Users.Update(userToShare);

        await _db.SaveChangesAsync(cancellationToken);

        foreach (var participantId in participantIds)
            await _bot
                .SendTextMessageAsync(
                    participantId,
                    $"✅ Доступ к бюдету с именем &quot;{budgetToShare.Name.EscapeHtml()}&quot; предоставлен {userToShare.GetFullNameLink()}" +
                    Environment.NewLine +
                    Environment.NewLine +
                    $"<i>Инициатор: {_currentUserService.TelegramUser.GetFullNameLink()}</i>",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);

        await _bot
            .SendTextMessageAsync(
                userToShare.Id,
                $"❗ Вам предоставлен доступ к бюджету &quot;{budgetToShare.Name.EscapeHtml()}&quot; пользователем {_currentUserService.TelegramUser.GetFullNameLink()}",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }
}