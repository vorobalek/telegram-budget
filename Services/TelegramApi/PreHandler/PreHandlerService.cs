using Telegram.Bot.Types;
using Telegram.Flow.Updates;

namespace TelegramBudget.Services.TelegramApi.PreHandler;

public class PreHandlerService(IUpdateHandler preHandler) : IPreHandlerService
{
    public Task PreHandleAsync(Update update, CancellationToken cancellationToken)
    {
        return preHandler.ProcessAsync(update, cancellationToken);
    }
}