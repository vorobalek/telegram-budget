using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBudget.Services.TelegramApi;

public static class Common
{
    public static readonly InlineKeyboardMarkup CmdAllInlineKeyboard = new(new[]
    {
        new[]
        {
            InlineKeyboardButton.WithCallbackData(TR.L + "START_COMMANDS", "cmd.all")
        }
    });

    public static readonly InlineKeyboardMarkup BackToMainInlineKeyboard = new(new[]
    {
        new[]
        {
            InlineKeyboardButton.WithCallbackData(TR.L + "BACK", "main")
        }
    });
}