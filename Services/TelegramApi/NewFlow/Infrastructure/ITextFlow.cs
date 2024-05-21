using Telegram.Flow.Updates.Messages.Texts;

namespace TelegramBudget.Services.TelegramApi.NewFlow.Infrastructure;

internal interface ITextFlow
{
    public Task ProcessAsync(ITextContext context, CancellationToken cancellationToken);
}