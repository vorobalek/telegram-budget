using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Configuration;
using TelegramBudget.Data;
using TelegramBudget.Extensions;
using User = TelegramBudget.Data.Entities.User;

namespace TelegramBudget.Services.TelegramUpdates;

internal sealed class HandleUpdateService(
    ICurrentUserService currentUserService,
    ApplicationDbContext db,
    ITelegramBotClient bot,
    IEnumerable<IUpdateHandler> handlers)
    : IHandleUpdateService
{
    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        currentUserService.TelegramUser = update.GetUser();
        await bot
            .SendChatActionAsync(
                currentUserService.TelegramUser.Id,
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
        await db.Users.AddAsync(new User
        {
            Id = currentUserService.TelegramUser.Id,
            FirstName = currentUserService.TelegramUser.FirstName,
            LastName = currentUserService.TelegramUser.LastName
        }, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        user.FirstName = currentUserService.TelegramUser.FirstName;
        user.LastName = currentUserService.TelegramUser.LastName;
        db.Update(user);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<User?> GetUserAsync(CancellationToken cancellationToken)
    {
        return await db.Users.SingleOrDefaultAsync(
            e => e.Id == currentUserService.TelegramUser.Id,
            cancellationToken);
    }

    private async Task SendUserIdAsync(CancellationToken cancellationToken)
    {
        await bot.SendTextMessageAsync(
            currentUserService.TelegramUser.Id,
            currentUserService.TelegramUser.Id.ToString(),
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);
    }

    private Task<bool> UserIsAuthorizedAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(TelegramBotConfiguration.AuthorizedUserIds.Contains(currentUserService.TelegramUser.Id));
    }

    private async Task ProcessUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        await Task.WhenAll(handlers
            .Where(e => e.TargetType == update.Type)
            .Select(e => e
                .ProcessAsync(update, cancellationToken)));
    }
}