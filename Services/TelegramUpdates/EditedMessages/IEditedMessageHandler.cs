using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBudget.Services.TelegramUpdates.EditedMessages;

public interface IEditedMessageHandler
{
    MessageType TargetType { get; }
    Task ProcessAsync(Message message, CancellationToken cancellationToken);
}

public class TextEditedMessageHandler(IEnumerable<ITextEditedHandler> handlers) : IEditedMessageHandler
{
    public MessageType TargetType => MessageType.Text;

    public Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        return message.Text is not null
            ? Task.WhenAll(handlers
                .Where(e => e.ShouldBeInvoked(message))
                .Select(e => e.ProcessAsync(message, cancellationToken)))
            : Task.CompletedTask;
    }
}