using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class MeTextHandler(
    ITelegramBotClient bot,
    ICurrentUserService currentUserService)
    : ITextHandler
{
    public bool ShouldBeInvoked(Message message)
    {
        return message.Text!.Trim().StartsWith("/me");
    }

    public Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        return bot
            .SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                currentUserService.TelegramUser.Id.ToString(),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }
}