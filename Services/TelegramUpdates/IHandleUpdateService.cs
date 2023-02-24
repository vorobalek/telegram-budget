using Telegram.Bot.Types;

namespace TelegramBudget.Services.TelegramUpdates;

public interface IHandleUpdateService
{
    Task HandleUpdateAsync(Update update, CancellationToken cancellationToken);
}