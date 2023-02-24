using Telegram.Bot.Types;

namespace TelegramBudget.Services.TelegramUpdates.EditedMessages;

public interface ITextEditedHandler
{
    bool ShouldBeInvoked(Message message);

    Task ProcessAsync(Message message, CancellationToken cancellationToken);
}