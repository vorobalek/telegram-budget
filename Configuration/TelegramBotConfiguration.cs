using Telegram.Bot.Types;

namespace TelegramBudget.Configuration;

public static class TelegramBotConfiguration
{
    public static readonly string BotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN")!;
    public static readonly string WebhookSecretToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_WEBHOOK_SECRET_TOKEN")!;
    public static readonly long[] AuthorizedUserIds = Environment.GetEnvironmentVariable("TELEGRAM_BOT_AUTHORIZED_USER_IDS")!
        .Trim()
        .Split(',',';',' ')
        .Select(x => x.Trim())
        .Where(x => long.TryParse(x, out _))
        .Select(long.Parse)
        .ToArray();

    public static readonly BotCommand[] Commands =
    {
        new()
        {
            Command = "/list",
            Description = TR.L+"LIST"
        },
        new()
        {
            Command = "/history",
            Description = TR.L+"HISTORY"
        },
        new()
        {
            Command = "/me",
            Description = TR.L+"ME"
        },
        new()
        {
            Command = "/create",
            Description = TR.L+"CREATE"
        },
        new()
        {
            Command = "/switch",
            Description = TR.L+"SWITCH"
        },
        new()
        {
            Command = "/grant",
            Description = TR.L+"GRANT"
        },
        new()
        {
            Command = "/timezone",
            Description = TR.L+"TZ"
        },
        new()
        {
            Command = "/revoke",
            Description = TR.L+"REVOKE"
        },
        new()
        {
            Command = "/delete",
            Description = TR.L+"DELETE"
        },
        new()
        {
            Command = "/start",
            Description = TR.L+"START"
        },
        new()
        {
            Command = "/help",
            Description = TR.L+"HELP"
        }
    };
}