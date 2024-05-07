using Telegram.Bot.Types;

namespace TelegramBudget.Services.TelegramApi.PreHandler;

public interface IPreHandlerService
{
    Task PreHandleAsync(Update update, CancellationToken cancellationToken);
}