using Telegram.Bot.Types;
using Tracee;

namespace TelegramBudget.Services.CurrentUser;

internal sealed class CurrentUserService(
    IHttpContextAccessor httpContextAccessor,
    ITracee tracee) : ICurrentUserService
{
    private const string CurrentUserKey = "__CURRENT_USER__";

    public User TelegramUser
    {
        get
        {
            using var trace = tracee.Scoped("current_user");
            return (httpContextAccessor.HttpContext?.Items.TryGetValue(CurrentUserKey, out var user) ?? false
                       ? user as User
                       : null)
                   ?? throw new InvalidOperationException("Current user have not been initialized yet");
        }
        set
        {
            if (httpContextAccessor.HttpContext is null)
                throw new InvalidOperationException("Current context have not been initialized");

            if (!httpContextAccessor.HttpContext.Items.TryAdd(CurrentUserKey, value))
                throw new InvalidOperationException("Current user have already been initialized");
        }
    }

    public User? TryGetTelegramUser()
    {
        return httpContextAccessor.HttpContext?.Items.TryGetValue(CurrentUserKey, out var user) ?? false
            ? user as User
            : null;
    }
}