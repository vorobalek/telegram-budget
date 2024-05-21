using Telegram.Flow.Updates.Messages.Texts.BotCommands;

namespace TelegramBudget.Services.TelegramApi.NewFlow;

internal interface IBotCommandFlow
{
    public Task ProcessAsync(IBotCommandContext context, CancellationToken cancellationToken);
}