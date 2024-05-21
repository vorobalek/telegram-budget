using Telegram.Bot.Types;
using TelegramBudget.Services.TelegramApi.NewFlow;

namespace TelegramBudget.Configuration;

public static class TelegramBotConfiguration
{
    public static readonly string BotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN")!;

    public static readonly string WebhookSecretToken =
        Environment.GetEnvironmentVariable("TELEGRAM_WEBHOOK_SECRET")!;

    public static readonly bool IsUserAuthorizationEnabled =
        Environment.GetEnvironmentVariable("AUTHORIZED_USER_IDS") != "*";

    public static readonly long[] AuthorizedUserIds =
        Environment.GetEnvironmentVariable("AUTHORIZED_USER_IDS")!
            .Trim()
            .Split(',', ';', ' ')
            .Select(x => x.Trim())
            .Where(x => long.TryParse(x, out _))
            .Select(long.Parse)
            .ToArray();

    public static readonly BotCommand[] DeclaredCommands =
    {
        new()
        {
            Command = $"{NewMain.Command}",
            Description = TR.L + "START"
        },
        new()
        {
            Command = "/help",
            Description = TR.L + "HELP"
        },
        new()
        {
            Command = "/list",
            Description = TR.L + "LIST"
        },
        new()
        {
            Command = "/me",
            Description = TR.L + "ME"
        },
        new()
        {
            Command = "/history",
            Description = TR.L + "HISTORY"
        },
        new()
        {
            Command = "/create",
            Description = TR.L + "CREATE"
        },
        new()
        {
            Command = "/switch",
            Description = TR.L + "SWITCH"
        },
        new()
        {
            Command = "/timezone",
            Description = TR.L + "TIMEZONE"
        },
        new()
        {
            Command = "/grant",
            Description = TR.L + "GRANT"
        },
        new()
        {
            Command = "/revoke",
            Description = TR.L + "REVOKE"
        },
        new()
        {
            Command = "/delete",
            Description = TR.L + "DELETE"
        }
    };
}