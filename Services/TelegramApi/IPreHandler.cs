using Telegram.Bot.Types;

namespace TelegramBudget.Services.TelegramApi;

public interface IPreHandler
{
    Task ProcessAsync(Update update, CancellationToken cancellationToken);
}