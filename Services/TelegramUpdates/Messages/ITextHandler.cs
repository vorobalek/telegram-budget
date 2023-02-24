using Telegram.Bot.Types;

namespace TelegramBudget.Services.TelegramUpdates.Messages;

public interface ITextHandler
{
    bool ShouldBeInvoked(Message message);

    Task ProcessAsync(Message message, CancellationToken cancellationToken);
}