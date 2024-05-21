using System.Data;
using Common.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Flow.Updates;
using TelegramBudget.Configuration;
using TelegramBudget.Data;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramApi.PreHandle;
using TelegramBudget.Services.TelegramApi.UserPrompt;
using Tracee;
using User = TelegramBudget.Data.Entities.User;

namespace TelegramBudget.Services.TelegramApi;

internal sealed class TelegramApiService(
    ITracee tracee,
    ICurrentUserService currentUserService,
    ApplicationDbContext db,
    IPreHandlerService preHandlerService,
    IUserPromptService userPromptService,
    IEnumerable<IUpdateFlow> flows)
    : ITelegramApiService
{
    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("handle");

        preHandlerService.PreHandleAsync(update, cancellationToken).DontWait(cancellationToken);
        
        await using var transaction = await db.Database
            .BeginTransactionAsync(
                IsolationLevel.Snapshot,
                cancellationToken);
        try
        {
            if (await GetUserAsync(cancellationToken) is not { } user)
                user = await CreateUserAsync(cancellationToken);

            if (TelegramBotConfiguration.IsUserAuthorizationEnabled &&
                !IsUserAuthorized())
                return;

            await UpdateUserAsync(user, cancellationToken);

            if (await userPromptService.ProcessPromptAsync(user, update, cancellationToken))
                await ProcessUpdateAsync(update, user, cancellationToken);
        
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            tracee.Logger.LogCritical(exception,"An error while processing the update. Rollback");
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<User?> GetUserAsync(CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("get_user");

        return await db.User.SingleOrDefaultAsync(
            user => user.Id == currentUserService.TelegramUser.Id,
            cancellationToken);
    }

    private async Task<User> CreateUserAsync(CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("create_user");

        var user = new User
        {
            Id = currentUserService.TelegramUser.Id,
            FirstName = currentUserService.TelegramUser.FirstName,
            LastName = currentUserService.TelegramUser.LastName
        };
        await db.User.AddAsync(user, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return user;
    }

    private async Task UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("update_user");

        user.FirstName = currentUserService.TelegramUser.FirstName;
        user.LastName = currentUserService.TelegramUser.LastName;
        db.Update(user);
        await db.SaveChangesAsync(cancellationToken);
    }

    private bool IsUserAuthorized()
    {
        using var _ = tracee.Scoped("auth_user");

        return TelegramBotConfiguration.AuthorizedUserIds.Contains(currentUserService.TelegramUser.Id);
    }

    private async Task ProcessUpdateAsync(Update update, User user, CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("process");

        await Task.WhenAll(flows.Select(flow => flow.ProcessAsync(update, cancellationToken)));
    }
}