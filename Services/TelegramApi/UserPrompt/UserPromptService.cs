using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Services.TelegramApi.NewFlow;
using Tracee;
using User = TelegramBudget.Data.Entities.User;

namespace TelegramBudget.Services.TelegramApi.UserPrompt;

internal sealed class UserPromptService(
    ITracee tracee,
    NewCancel cancelFlow,
    IEnumerable<IUserPromptFlow> promptFlows) : IUserPromptService
{
    public async Task<bool> ProcessPromptAsync(User user, Update update, CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("prompt");

        if (user.PromptSubject is not { } promptSubject)
            return true;

        if (IsCancelBotCommand(update))
        {
            await cancelFlow.ProcessAsync(cancellationToken);
        }
        else
        {
            await Task.WhenAll(promptFlows
                .Where(e => e.SubjectType == promptSubject)
                .Select(e => e.ProcessPromptAsync(user, update, cancellationToken)));
        }

        return false;
    }

    private static bool IsCancelBotCommand(Update update)
    {
        const string cancelCommand = $"/{NewCancel.Command}";
        return update is
        {
            Type: UpdateType.Message,
            Message:
            {
                Type: MessageType.Text,
                Text: { } text,
                Entities:
                [
                    {
                        Type: MessageEntityType.BotCommand,
                        Offset: 0,
                        Length: var length
                    }
                ]
            }
        } && length == cancelCommand.Length && text == cancelCommand;
    }
}