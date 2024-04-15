using Telegram.Bot.Types;

namespace TelegramBudget.Services.CurrentUser;

public interface ICurrentUserService
{
    User TelegramUser { get; set; }

    User? TryGetTelegramUser();
}