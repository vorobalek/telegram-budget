using Telegram.Bot.Types.Enums;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handlers;

public sealed class StartBotCommand(
    ITelegramBotClientWrapper botWrapper,
    ICurrentUserService currentUserService)
{
    public Task ProcessAsync(CancellationToken cancellationToken)
    {
        return botWrapper
            .SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                TR.L + "HELP_GREETING",
                parseMode: ParseMode.Html,
                disableWebPagePreview: true,
                replyMarkup: Keyboards.CmdAllInline,
                cancellationToken: cancellationToken);
    }
}