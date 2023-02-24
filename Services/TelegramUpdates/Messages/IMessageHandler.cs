using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBudget.Services.TelegramUpdates.Messages;

public interface IMessageHandler
{
    MessageType TargetType { get; }
    Task ProcessAsync(Message message, CancellationToken cancellationToken);
}