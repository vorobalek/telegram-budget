namespace TelegramBudget.Configuration;

public static class AppConfiguration
{
    private static readonly string Domain = Environment.GetEnvironmentVariable("DOMAIN")!;
    public static readonly string Url = $"https://{Domain}";
    public static readonly string Locale = Environment.GetEnvironmentVariable("LOCALE") ?? "ru";
    public static readonly string DateFormat = Environment.GetEnvironmentVariable("DATE_FORMAT") ?? "dd.MM.yyyy";
    public static readonly string DateTimeFormat = Environment.GetEnvironmentVariable("DATETIME_FORMAT") ?? "dd.MM.yyyy HH:mm";
    public static readonly string Port = Environment.GetEnvironmentVariable("PORT")!;
    public static readonly string ConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")!;
}