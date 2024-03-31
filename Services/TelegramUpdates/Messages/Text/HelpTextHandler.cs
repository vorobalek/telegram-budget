using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Configuration;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class HelpTextHandler(
    ITelegramBotClient bot,
    ICurrentUserService currentUserService)
    : ITextHandler
{
    public bool ShouldBeInvoked(Message message)
    {
        return message.Text!.Trim().StartsWith("/help") || message.Text!.Trim().StartsWith("/start");
    }

    public Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(TR.L+"HELP_INTRO");
        stringBuilder.AppendLine();

        foreach (var command in TelegramBotConfiguration.Commands)
        {
            stringBuilder.AppendLine(command.Description);
            stringBuilder.AppendLine();
        }

        stringBuilder.Append(TR.L+"HELP_DETAILS");

        return bot
            .SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                stringBuilder.ToString(),
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);
    }
}