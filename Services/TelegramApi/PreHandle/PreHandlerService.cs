using System.Diagnostics;
using Telegram.Bot.Types;
using Telegram.Flow.Updates;
using Tracee;

namespace TelegramBudget.Services.TelegramApi.PreHandle;

[DebuggerDisplay(nameof(PreHandlerService))]
internal sealed class PreHandlerService(
    ITracee tracee,
    IUpdateFlow preHandler) : IPreHandlerService
{
    public async Task PreHandleAsync(Update update, CancellationToken cancellationToken)
    {
        using var _ = tracee.Fixed("send_chat_action_total");

        await preHandler.ProcessAsync(update, cancellationToken);
    }
}