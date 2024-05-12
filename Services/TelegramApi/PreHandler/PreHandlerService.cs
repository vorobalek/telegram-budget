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
    public Task PreHandleAsync(Update update, CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            using (tracee.Fixed("prehandler_total"))
            {
                try
                {
                    await preHandler.ProcessAsync(update, cancellationToken);
                }
                catch
                {
                    // ignored, fire-n-forget
                }
            }
        }, cancellationToken);
        return Task.CompletedTask;
    }
}