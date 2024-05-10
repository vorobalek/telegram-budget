namespace TelegramBudget.Services.TelegramApi.NewHandlers;

public interface IBotCommandHandler
{
    public Task ProcessAsync(string data, CancellationToken cancellationToken);
}