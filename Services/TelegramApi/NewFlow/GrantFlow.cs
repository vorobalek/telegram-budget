using Telegram.Bot;
using Telegram.Flow.Updates.CallbackQueries.Data;
using TelegramBudget.Services.TelegramApi.NewFlow.Infrastructure;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.NewFlow;

internal sealed class GrantFlow(ITelegramBotWrapper botWrapper) : ICallbackQueryFlow
{
    public const string Command = "grant";
    public const string CommandPrefix = "grant.";
    
    public async Task ProcessAsync(IDataContext context, CancellationToken cancellationToken)
    {
        await botWrapper
            .BotClient
            .AnswerCallbackQuery(
                context.CallbackQuery.Id,
                TR.L + "_NOT_IMPLEMENTED",
                showAlert: true,
                cancellationToken: cancellationToken);
    }
}