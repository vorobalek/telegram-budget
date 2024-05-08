using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handlers;

public sealed class MainCallback(
    ITelegramBotClientWrapper bot,
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
                replyMarkup: Keyboards.CmdAllInline,
                cancellationToken: cancellationToken);
    }
}