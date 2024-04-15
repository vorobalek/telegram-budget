using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Services.CurrentUser;

namespace TelegramBudget.Services.TelegramApi.Handlers;

public sealed class StartBotCommand(
    ITelegramBotClient bot,
    ICurrentUserService currentUserService)
{
    public Task ProcessAsync(CancellationToken cancellationToken)
    {
        return bot
            .SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                TR.L + "START_GREETING",
                parseMode: ParseMode.Html,
                disableWebPagePreview: true,
                replyMarkup: Common.CmdAllInlineKeyboard,
                cancellationToken: cancellationToken);
    }
}