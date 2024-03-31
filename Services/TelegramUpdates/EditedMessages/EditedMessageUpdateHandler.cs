using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBudget.Services.TelegramUpdates.EditedMessages;

public class EditedMessageUpdateHandler(IEnumerable<IEditedMessageHandler> handlers) : IUpdateHandler
{
    public UpdateType TargetType => UpdateType.EditedMessage;

    public Task ProcessAsync(Update update, CancellationToken cancellationToken)
    {
        return update.EditedMessage is not null
            ? Task.WhenAll(handlers
                .Where(e => e.TargetType == update.EditedMessage.Type)
                .Select(e => e.ProcessAsync(update.EditedMessage, cancellationToken)))
            : Task.CompletedTask;
    }
}