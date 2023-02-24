namespace TelegramBudget.Configuration;

public static class SentryConfiguration
{
    public const LogLevel MinimumEventLevel = LogLevel.Error;
    public static readonly string Dsn = Environment.GetEnvironmentVariable("SENTRY_DSN")!;
}