using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Services.CurrentUser;

namespace TelegramBudget.Services.TelegramApi.Handlers;

public sealed class MainCallback(
    ITelegramBotClient bot,
    ICurrentUserService currentUserService)
{
    public Task ProcessAsync(
        Message? callbackQueryMessage, 
        CancellationToken cancellationToken)
    {
        if (callbackQueryMessage is null) return Task.CompletedTask;

        return bot
            .EditMessageTextAsync(
                currentUserService.TelegramUser.Id,
                callbackQueryMessage.MessageId,
                TR.L + "START_GREETING",
                parseMode: ParseMode.Html,
                replyMarkup: Common.CmdAllInlineKeyboard,
                cancellationToken: cancellationToken);
    }
}