namespace TelegramBudget.Services;

public class GlobalCancellationTokenSource
{
    private readonly CancellationTokenSource _tokenSource = new();

    public GlobalCancellationTokenSource(IHostApplicationLifetime lifetime)
    {
        lifetime.ApplicationStopping.Register(_tokenSource.Cancel);
    }

    public CancellationToken CancellationToken => _tokenSource.Token;
}