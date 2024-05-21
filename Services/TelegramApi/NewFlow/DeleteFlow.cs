using Telegram.Bot;
using Telegram.Flow.Updates.CallbackQueries.Data;
using TelegramBudget.Services.TelegramApi.NewFlow.Infrastructure;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.NewFlow;

internal sealed class DeleteFlow(ITelegramBotWrapper botWrapper) : ICallbackQueryFlow
{
    public const string Command = "delete";
    public const string CommandPrefix = "delete.";
    
    public async Task ProcessAsync(IDataContext context, CancellationToken cancellationToken)
    {
        await botWrapper
            .BotClient
            .AnswerCallbackQueryAsync(
                context.CallbackQuery.Id,
                TR.L + "_NOT_IMPLEMENTED",
                showAlert: true,
                cancellationToken: cancellationToken);
    }
}