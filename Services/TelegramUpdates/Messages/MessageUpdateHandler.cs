using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBudget.Services.TelegramUpdates.Messages;

public class MessageUpdateHandler(IEnumerable<IMessageHandler> handlers) : IUpdateHandler
{
    public UpdateType TargetType => UpdateType.Message;

    public Task ProcessAsync(Update update, CancellationToken cancellationToken)
    {
        return update.Message is not null
            ? Task.WhenAll(handlers
                .Where(e => e.TargetType == update.Message.Type)
                .Select(e => e.ProcessAsync(update.Message, cancellationToken)))
            : Task.CompletedTask;
    }
}