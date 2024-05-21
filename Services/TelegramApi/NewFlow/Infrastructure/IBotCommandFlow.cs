using Telegram.Flow.Updates.Messages.Texts.BotCommands;

namespace TelegramBudget.Services.TelegramApi.NewFlow.Infrastructure;

internal interface IBotCommandFlow
{
    public Task ProcessAsync(IBotCommandContext context, CancellationToken cancellationToken);
}