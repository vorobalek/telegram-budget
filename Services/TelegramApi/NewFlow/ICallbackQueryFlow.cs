namespace TelegramBudget.Services.TelegramApi.NewFlow;

internal interface ICallbackQueryFlow
{
    public Task ProcessAsync(int messageId, string data, CancellationToken cancellationToken);
}