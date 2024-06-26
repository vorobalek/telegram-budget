namespace TelegramBudget.Services;

public sealed class GlobalCancellationTokenSource
{
    private readonly CancellationTokenSource _tokenSource = new();

    public GlobalCancellationTokenSource(IHostApplicationLifetime lifetime)
    {
        lifetime.ApplicationStopping.Register(_tokenSource.Cancel);
    }

    public CancellationToken Token => _tokenSource.Token;
}