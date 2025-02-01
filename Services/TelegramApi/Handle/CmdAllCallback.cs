using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Configuration;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handle;

internal sealed class CmdAllCallback(
    ITelegramBotWrapper botWrapper,
    ICurrentUserService currentUserService)
{
    public async Task ProcessAsync(
        Message? callbackQueryMessage,
        CancellationToken cancellationToken)
    {
        if (callbackQueryMessage is null) return;

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

        await botWrapper
            .EditMessageText(
                currentUserService.TelegramUser.Id,
                callbackQueryMessage.Id,
                stringBuilder.ToString(),
                ParseMode.Html,
                linkPreviewOptions: new LinkPreviewOptions
                {
                    IsDisabled = true
                },
                replyMarkup: Keyboards.BackToMainInlineOld,
                cancellationToken: cancellationToken);
    }
}