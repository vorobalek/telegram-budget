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
            Command = "/me",
            Description = "/me - Получить свой номер."
        },
        new()
        {
            Command = "/timezone",
            Description =
                "/timezone <смещение от UTC в формате 00:00> - Установить свой часовой пояс. Требуется указать смещение от UTC."
        },
        new()
        {
            Command = "/create",
            Description =
                "/create <название бюджета> - Создать ноый бюджет. Новый бюджет автоматически станет активным."
        },
        new()
        {
            Command = "/switch",
            Description = "/switch <название бюджета> - Переключить активный бюджет."
        },
        new()
        {
            Command = "/share",
            Description =
                "/share <номер пользователя> (<название бюджета>) - Предоставить доступ к бюджету другому пользователю. Название бюджета не обязательно. Без указания названия бюджета будет выбран активный бюджет."
        },
        new()
        {
            Command = "/unshare",
            Description =
                "/unshare <номер пользователя> (<название бюджета>) - Отозвать доступ к бюджету у другого пользователя. Название бюджета не обязательно. Без указания названия бюджета будет выбран активный бюджет."
        },
        new()
        {
            Command = "/history",
            Description =
                "/history (<название бюджета>) - Получить историю транзакций по бюджету. Название бюджета не обязательно. Без указания названия бюджета будет выбран активный бюджет."
        },
        new()
        {
            Command = "/list",
            Description = "/list - Получить список доступных бюджетов."
        },
        new()
        {
            Command = "/remove",
            Description =
                "/remove (<название бюджета>) - Удалить бюджет. Все транзакции будут безвозвратно удалены. Название бюджета не обязательно. Без указания названия бюджета будет выбран активный бюджет."
        },
        new()
        {
            Command = "/start",
            Description = "/start - Показать справку."
        },
        new()
        {
            Command = "/help",
            Description = "/help - Показать справку."
        }
    };
}