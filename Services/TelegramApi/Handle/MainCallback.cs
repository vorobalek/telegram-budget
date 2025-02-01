using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handle;

internal sealed class MainCallback(
    ITelegramBotWrapper botWrapper,
    ICurrentUserService currentUserService)
{
    public async Task ProcessAsync(
        Message? callbackQueryMessage,
        CancellationToken cancellationToken)
    {
        if (callbackQueryMessage is null) return;

        await botWrapper
            .EditMessageText(
                currentUserService.TelegramUser.Id,
                callbackQueryMessage.Id,
                TR.L + "HELP_GREETING",
                ParseMode.Html,
                replyMarkup: Keyboards.CmdAllInline,
                cancellationToken: cancellationToken);
    }
}