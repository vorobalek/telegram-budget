using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Configuration;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handlers;

public sealed class CmdAllCallback(
    ITelegramBotClientWrapper botWrapper,
    ICurrentUserService currentUserService)
{
    public Task ProcessAsync(
        Message? callbackQueryMessage,
        CancellationToken cancellationToken)
    {
        if (callbackQueryMessage is null) return Task.CompletedTask;

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(TR.L + "HELP_INTRO");
        stringBuilder.AppendLine();

        foreach (var command in TelegramBotConfiguration.DeclaredCommands)
        {
            var commandExampleTranslationKey =
                command.Command
                    .TrimStart('/')
                    .Split(' ')
                    .First()
                    .ToUpper()
                + "_EXAMPLE";
            var commandExampleOrDescription = TR.L & commandExampleTranslationKey
                ? TR.L + commandExampleTranslationKey
                : $"{command.Command} {command.Description}";
            stringBuilder.AppendLine(commandExampleOrDescription);
            stringBuilder.AppendLine();
        }

        stringBuilder.Append(TR.L + "HELP_OUTRO");

        return botWrapper
            .EditMessageTextAsync(
                currentUserService.TelegramUser.Id,
                callbackQueryMessage.MessageId,
                stringBuilder.ToString(),
                ParseMode.Html,
                disableWebPagePreview: true,
                replyMarkup: Keyboards.BackToMainInlineOld,
                cancellationToken: cancellationToken);
    }
}