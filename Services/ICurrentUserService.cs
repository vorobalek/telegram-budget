using Telegram.Bot.Types;

namespace TelegramBudget.Services;

public interface ICurrentUserService
{
    User TelegramUser { get; set; }
}