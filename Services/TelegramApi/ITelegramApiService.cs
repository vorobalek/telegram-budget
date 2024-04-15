using Telegram.Bot.Types;

namespace TelegramBudget.Services.TelegramApi;

public interface ITelegramApiService
{
    Task HandleUpdateAsync(Update update, CancellationToken cancellationToken);
}