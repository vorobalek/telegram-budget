using Telegram.Bot.Types;

namespace TelegramBudget.Services.TelegramApi.PreHandle;

internal interface IPreHandlerService
{
    Task PreHandleAsync(Update update, CancellationToken cancellationToken);
}