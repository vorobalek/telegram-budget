using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class MeTextHandler : ITextHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly ICurrentUserService _currentUserService;

    public MeTextHandler(
        ITelegramBotClient bot,
        ICurrentUserService currentUserService)
    {
        _bot = bot;
        _currentUserService = currentUserService;
    }

    public bool ShouldBeInvoked(Message message)
    {
        return message.Text!.Trim().StartsWith("/me");
    }

    public Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        return _bot
            .SendTextMessageAsync(
                _currentUserService.TelegramUser.Id,
                _currentUserService.TelegramUser.Id.ToString(),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }
}