using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBudget.Services.TelegramUpdates;

public interface IUpdateHandler
{
    UpdateType TargetType { get; }
    Task ProcessAsync(Update update, CancellationToken cancellationToken);
}