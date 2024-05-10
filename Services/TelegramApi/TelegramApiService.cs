using System.Data;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Flow.Updates;
using TelegramBudget.Configuration;
using TelegramBudget.Data;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramApi.PreHandler;
using User = TelegramBudget.Data.Entities.User;

namespace TelegramBudget.Services.TelegramApi;

internal sealed class TelegramApiService(
    ICurrentUserService currentUserService,
    ApplicationDbContext db,
    IPreHandlerService preHandlerService,
    IEnumerable<IUpdateHandler> handlers)
    : ITelegramApiService
{
    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database
            .BeginTransactionAsync(IsolationLevel.Snapshot, cancellationToken);

        if (await GetUserAsync(cancellationToken) is not { } user) user = await CreateUserAsync(cancellationToken);

        if (TelegramBotConfiguration.IsUserAuthorizationEnabled && !await IsUserAuthorizedAsync(cancellationToken))
            return;

        await UpdateUserAsync(user, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        await ProcessUpdateAsync(update, cancellationToken);
    }

    private async Task<User?> GetUserAsync(CancellationToken cancellationToken)
    {
        return await db.Users.SingleOrDefaultAsync(
            user => user.Id == currentUserService.TelegramUser.Id,
            cancellationToken);
    }

    private async Task<User> CreateUserAsync(CancellationToken cancellationToken)
    {
        var user = new User
        {
            Id = currentUserService.TelegramUser.Id,
            FirstName = currentUserService.TelegramUser.FirstName,
            LastName = currentUserService.TelegramUser.LastName
        };
        await db.Users.AddAsync(user, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return user;
    }

    private async Task UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        user.FirstName = currentUserService.TelegramUser.FirstName;
        user.LastName = currentUserService.TelegramUser.LastName;
        db.Update(user);
        await db.SaveChangesAsync(cancellationToken);
    }

    private Task<bool> IsUserAuthorizedAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(TelegramBotConfiguration.AuthorizedUserIds.Contains(currentUserService.TelegramUser.Id));
    }

    private async Task ProcessUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        await preHandlerService.PreHandleAsync(update, cancellationToken);
        await Task.WhenAll(handlers.Select(handler => handler.ProcessAsync(update, cancellationToken)));
    }
}