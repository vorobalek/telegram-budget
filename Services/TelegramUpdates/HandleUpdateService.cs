using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Configuration;
using TelegramBudget.Data;
using TelegramBudget.Extensions;
using User = TelegramBudget.Data.Entities.User;

namespace TelegramBudget.Services.TelegramUpdates;

internal sealed class HandleUpdateService : IHandleUpdateService
{
    private readonly ITelegramBotClient _bot;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _db;
    private readonly IEnumerable<IUpdateHandler> _handlers;

    public HandleUpdateService(
        ICurrentUserService currentUserService,
        ApplicationDbContext db,
        ITelegramBotClient bot,
        IEnumerable<IUpdateHandler> handlers)
    {
        _currentUserService = currentUserService;
        _db = db;
        _bot = bot;
        _handlers = handlers;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        _currentUserService.TelegramUser = update.GetUser();
        await _bot
            .SendChatActionAsync(
                _currentUserService.TelegramUser.Id,
                ChatAction.Typing,
                cancellationToken: cancellationToken);

        if (await GetUserAsync(cancellationToken) is not { } user)
        {
            await CreateUserAsync(cancellationToken);
            await SendUserIdAsync(cancellationToken);
            return;
        }

        if (!await UserIsAuthorizedAsync(cancellationToken))
            return;

        await UpdateUserAsync(user, cancellationToken);
        await ProcessUpdateAsync(update, cancellationToken);
    }

    private async Task CreateUserAsync(CancellationToken cancellationToken)
    {
        await _db.Users.AddAsync(new User
        {
            Id = _currentUserService.TelegramUser.Id,
            FirstName = _currentUserService.TelegramUser.FirstName,
            LastName = _currentUserService.TelegramUser.LastName
        }, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        user.FirstName = _currentUserService.TelegramUser.FirstName;
        user.LastName = _currentUserService.TelegramUser.LastName;
        _db.Update(user);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<User?> GetUserAsync(CancellationToken cancellationToken)
    {
        return await _db.Users.SingleOrDefaultAsync(
            e => e.Id == _currentUserService.TelegramUser.Id,
            cancellationToken);
    }

    private async Task SendUserIdAsync(CancellationToken cancellationToken)
    {
        await _bot.SendTextMessageAsync(
            _currentUserService.TelegramUser.Id,
            _currentUserService.TelegramUser.Id.ToString(),
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);
    }

    private Task<bool> UserIsAuthorizedAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(TelegramBotConfiguration.AuthorizedUserIds.Contains(_currentUserService.TelegramUser.Id));
    }

    private async Task ProcessUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        await Task.WhenAll(_handlers
            .Where(e => e.TargetType == update.Type)
            .Select(e => e
                .ProcessAsync(update, cancellationToken)));
    }
}