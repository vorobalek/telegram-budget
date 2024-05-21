namespace TelegramBudget.Services.TelegramApi.NewFlow;

internal interface IBotCommandFlow
{
    public Task ProcessAsync(string data, CancellationToken cancellationToken);
}