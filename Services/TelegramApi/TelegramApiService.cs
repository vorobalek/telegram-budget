using System.Data;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Flow.Updates;
using TelegramBudget.Configuration;
using TelegramBudget.Data;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramApi.PreHandler;
using Tracee;
using User = TelegramBudget.Data.Entities.User;

namespace TelegramBudget.Services.TelegramApi;

internal sealed class TelegramApiService(
    ICurrentUserService currentUserService,
    ApplicationDbContext db,
    IPreHandlerService preHandlerService,
    IEnumerable<IUpdateHandler> handlers,
    ITracee tracee)
    : ITelegramApiService
{
    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        using var scope = tracee.Scoped("api");

        await using var transaction = await db.Database
            .BeginTransactionAsync(IsolationLevel.Snapshot, cancellationToken);

        if (await GetUserAsync(scope, cancellationToken) is not { } user)
            user = await CreateUserAsync(scope, cancellationToken);

        if (TelegramBotConfiguration.IsUserAuthorizationEnabled &&
            !IsUserAuthorized(scope))
            return;

        await UpdateUserAsync(scope, user, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        await ProcessUpdateAsync(scope, update, cancellationToken);
    }

    private async Task<User?> GetUserAsync(ITracee trace, CancellationToken cancellationToken)
    {
        using var subScope = trace.Scoped("get_user");
        return await db.Users.SingleOrDefaultAsync(
            user => user.Id == currentUserService.TelegramUser.Id,
            cancellationToken);
    }

    private async Task<User> CreateUserAsync(ITracee trace, CancellationToken cancellationToken)
    {
        using var subScope = trace.Scoped("create_user");
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

    private async Task UpdateUserAsync(ITracee trace, User user, CancellationToken cancellationToken)
    {
        using var subScope = trace.Scoped("update_user");
        user.FirstName = currentUserService.TelegramUser.FirstName;
        user.LastName = currentUserService.TelegramUser.LastName;
        db.Update(user);
        await db.SaveChangesAsync(cancellationToken);
    }

    private bool IsUserAuthorized(ITracee trace)
    {
        using var subScope = trace.Scoped("auth_user");
        return TelegramBotConfiguration.AuthorizedUserIds.Contains(currentUserService.TelegramUser.Id);
    }

    private async Task ProcessUpdateAsync(ITracee trace, Update update, CancellationToken cancellationToken)
    {
        using (trace.Scoped("prehandle"))
        {
            try
            {
                _ = Task.Run(async () =>
                    await preHandlerService.PreHandleAsync(update, cancellationToken), cancellationToken);
            }
            catch
            {
                //ignored fire-n-forget
            }
        }

        using (trace.Scoped("handle"))
        {
            await Task.WhenAll(handlers.Select(handler => handler.ProcessAsync(update, cancellationToken)));
        }
    }
}