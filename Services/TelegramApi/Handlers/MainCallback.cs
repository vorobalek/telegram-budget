using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handlers;

public sealed class MainCallback(
    ITelegramBotClientWrapper botWrapper,
    ICurrentUserService currentUserService)
{
    public async Task ProcessAsync(
        Message? callbackQueryMessage,
        CancellationToken cancellationToken)
    {
        if (callbackQueryMessage is null) return;

        await botWrapper
            .EditMessageTextAsync(
                currentUserService.TelegramUser.Id,
                callbackQueryMessage.MessageId,
                TR.L + "HELP_GREETING",
                ParseMode.Html,
                replyMarkup: Keyboards.CmdAllInline,
                cancellationToken: cancellationToken);
    }
}