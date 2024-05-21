using Telegram.Bot.Types;

namespace TelegramBudget.Services.TelegramApi.NewFlow;

internal interface ITextFlow
{
    public Task ProcessAsync(Message message, string text, CancellationToken cancellationToken);
}