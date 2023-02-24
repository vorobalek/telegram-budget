using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Configuration;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class HelpTextHandler : ITextHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly ICurrentUserService _currentUserService;

    public HelpTextHandler(
        ITelegramBotClient bot,
        ICurrentUserService currentUserService)
    {
        _bot = bot;
        _currentUserService = currentUserService;
    }

    public bool ShouldBeInvoked(Message message)
    {
        return message.Text!.Trim().StartsWith("/help") || message.Text!.Trim().StartsWith("/start");
    }

    public Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("*Бот поддерживает следующие команды:*");
        stringBuilder.AppendLine();

        foreach (var command in TelegramBotConfiguration.Commands)
        {
            stringBuilder.AppendLine(command.Description);
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine("Любое сообщения вида: *<число> <комментарий>* (например, *-100 за кофе*) будет " +
                                 "зафиксировано как транзакция на указаную сумму с указаным комментарием. " +
                                 "Коментарий не обязателен.");
        stringBuilder.AppendLine();
        stringBuilder.Append("Любую транзакцию можно отредактировать, отредактировав отправленное сообщение, " +
                             "в таком случае все участники бюджета получат уведомление об изменении. Допускается " +
                             "редактирование суммы и комментария. Отредактировать может только тот же пользователь, " +
                             "что создал транзакцию.");

        return _bot
            .SendTextMessageAsync(
                _currentUserService.TelegramUser.Id,
                stringBuilder.ToString(),
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);
    }
}