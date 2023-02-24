namespace TelegramBudget.Configuration;

public static class AppConfiguration
{
    public static readonly string Domain = Environment.GetEnvironmentVariable("DOMAIN")!;
    public static readonly string Url = $"https://{Domain}";
    public static readonly string Port = Environment.GetEnvironmentVariable("PORT")!;
    public static readonly string ConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")!;

}