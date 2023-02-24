using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBudget.Services.TelegramUpdates.Messages;

public class TextMessageHandler : IMessageHandler
{
    private readonly IEnumerable<ITextHandler> _handlers;

    public TextMessageHandler(IEnumerable<ITextHandler> handlers)
    {
        _handlers = handlers;
    }

    public MessageType TargetType => MessageType.Text;

    public Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        return message.Text is not null
            ? Task.WhenAll(_handlers
                .Where(e => e.ShouldBeInvoked(message))
                .Select(e => e.ProcessAsync(message, cancellationToken)))
            : Task.CompletedTask;
    }
}