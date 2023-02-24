using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBudget.Services.TelegramUpdates.EditedMessages;

public class EditedMessageUpdateHandler : IUpdateHandler
{
    private readonly IEnumerable<IEditedMessageHandler> _handlers;

    public EditedMessageUpdateHandler(IEnumerable<IEditedMessageHandler> handlers)
    {
        _handlers = handlers;
    }

    public UpdateType TargetType => UpdateType.EditedMessage;

    public Task ProcessAsync(Update update, CancellationToken cancellationToken)
    {
        return update.EditedMessage is not null
            ? Task.WhenAll(_handlers
                .Where(e => e.TargetType == update.EditedMessage.Type)
                .Select(e => e.ProcessAsync(update.EditedMessage, cancellationToken)))
            : Task.CompletedTask;
    }
}