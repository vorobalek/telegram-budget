using Telegram.Bot.Types.Enums;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handlers;

public sealed class MeBotCommand(
    ITelegramBotClientWrapper bot,
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