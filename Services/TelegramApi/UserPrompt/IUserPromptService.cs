using Telegram.Bot.Types;
using User = TelegramBudget.Data.Entities.User;

namespace TelegramBudget.Services.TelegramApi.UserPrompt;

internal interface IUserPromptService
{
    Task<bool> ProcessPromptAsync(User user, Update update, CancellationToken cancellationToken);
}