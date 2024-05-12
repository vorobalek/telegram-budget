using Telegram.Bot.Types.Enums;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handlers;

public sealed class MeBotCommand(
    ITelegramBotClientWrapper botWrapper,
    ICurrentUserService currentUserService)
{
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await botWrapper
            .SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                $"<code>{currentUserService.TelegramUser.Id}</code>",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }
}