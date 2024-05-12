using System.Diagnostics;
using Telegram.Bot.Types;
using Telegram.Flow.Updates;
using Tracee;

namespace TelegramBudget.Services.TelegramApi.PreHandler;

[DebuggerDisplay(nameof(PreHandlerService))]
public class PreHandlerService(
    IUpdateHandler preHandler,
    ITracee tracee) : IPreHandlerService
{
    public async Task PreHandleAsync(Update update, CancellationToken cancellationToken)
    {
        using (tracee.Fixed("prehandler_total"))
        {
            await preHandler.ProcessAsync(update, cancellationToken);
        }
    }
}