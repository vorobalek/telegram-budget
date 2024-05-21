using Telegram.Flow.Updates.CallbackQueries.Data;

namespace TelegramBudget.Services.TelegramApi.NewFlow;

internal interface ICallbackQueryFlow
{
    public Task ProcessAsync(IDataContext context, CancellationToken cancellationToken);
}