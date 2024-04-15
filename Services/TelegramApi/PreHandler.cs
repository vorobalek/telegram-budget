using Telegram.Bot.Types;
using Telegram.Flow.Updates;

namespace TelegramBudget.Services.TelegramApi;

public class PreHandler(IUpdateHandler preHandler) : IPreHandler
{
    public Task ProcessAsync(Update update, CancellationToken cancellationToken)
    {
        return preHandler.ProcessAsync(update, cancellationToken);
    }
}