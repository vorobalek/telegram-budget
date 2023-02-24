using Telegram.Bot.Types;

namespace TelegramBudget.Services;

internal sealed class CurrentUserService : ICurrentUserService
{
    private const string CurrentUserKey = "__CURRENT_USER__";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public User TelegramUser
    {
        get => (_httpContextAccessor.HttpContext?.Items.TryGetValue(CurrentUserKey, out var user) ?? false
                   ? user as User
                   : null)
               ?? throw new InvalidOperationException("Current user have not been initialized yet");
        set
        {
            if (_httpContextAccessor.HttpContext is null)
                throw new InvalidOperationException("Current context have not been initialized");

            if (_httpContextAccessor.HttpContext.Items.ContainsKey(CurrentUserKey))
                throw new InvalidOperationException("Current user have already been initialized");

            _httpContextAccessor.HttpContext.Items.Add(CurrentUserKey, value);
        }
    }
}