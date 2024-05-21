using Telegram.Bot.Types.Enums;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handle;

internal sealed class StartBotCommand(
    ITelegramBotWrapper botWrapper,
    ICurrentUserService currentUserService)
{
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await botWrapper
            .SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                TR.L + "HELP_GREETING",
                parseMode: ParseMode.Html,
                disableWebPagePreview: true,
                replyMarkup: Keyboards.CmdAllInline,
                cancellationToken: cancellationToken);
    }
}