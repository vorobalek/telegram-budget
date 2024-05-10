namespace TelegramBudget.Services.TelegramApi.NewHandlers;

public interface ICallbackQueryHandler
{
    public Task ProcessAsync(int messageId, string data, CancellationToken cancellationToken);
}