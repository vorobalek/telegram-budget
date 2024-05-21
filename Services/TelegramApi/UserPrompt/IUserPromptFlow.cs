using Telegram.Bot.Types;
using TelegramBudget.Data.Entities;
using User = TelegramBudget.Data.Entities.User;

namespace TelegramBudget.Services.TelegramApi.UserPrompt;

internal interface IUserPromptFlow
{
    UserPromptSubjectType SubjectType { get; }
    
    Task ProcessPromptAsync(User user, Update update, CancellationToken cancellationToken);
}