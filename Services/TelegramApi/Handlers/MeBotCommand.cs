using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Services.CurrentUser;

namespace TelegramBudget.Services.TelegramApi.Handlers;

public sealed class MeBotCommand(
    ITelegramBotClient bot,
    ICurrentUserService currentUserService)
{
    public Task ProcessAsync(CancellationToken cancellationToken)
    {
        return bot
            .SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                $"<code>{currentUserService.TelegramUser.Id}</code>",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }
}