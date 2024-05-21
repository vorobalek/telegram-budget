using Telegram.Flow.Updates.CallbackQueries.Data;

namespace TelegramBudget.Services.TelegramApi.NewFlow.Infrastructure;

internal interface ICallbackQueryFlow
{
    public Task ProcessAsync(IDataContext context, CancellationToken cancellationToken);
}