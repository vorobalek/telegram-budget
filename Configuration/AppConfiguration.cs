using TelegramBudget.Extensions;

namespace TelegramBudget.Configuration;

public static class AppConfiguration
{
    private static readonly string Domain = Environment.GetEnvironmentVariable("DOMAIN")!;
    public static readonly string? PathBase = Environment.GetEnvironmentVariable("PATH_BASE");
    public static readonly string Url = $"https://{Domain}";
    public static readonly string Locale = Environment.GetEnvironmentVariable("LOCALE").WithFallbackValue("en");

    public static readonly string DateTimeFormat =
        Environment.GetEnvironmentVariable("DATETIME_FORMAT").WithFallbackValue("hh:mm tt MM/dd/yyyy");

    public static readonly string Port = Environment.GetEnvironmentVariable("PORT")!;
    public static readonly string? DbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

    public static readonly bool DebugResponseTime =
        bool.TryParse(Environment.GetEnvironmentVariable("DEBUG_RESPONSE_TIME"), out var debugResponseTime) &&
        debugResponseTime;
}