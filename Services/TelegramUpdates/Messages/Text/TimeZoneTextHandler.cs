using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class TimeZoneTextHandler : ITextHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _db;

    public TimeZoneTextHandler(
        ITelegramBotClient bot,
        ICurrentUserService currentUserService,
        ApplicationDbContext db)
    {
        _bot = bot;
        _currentUserService = currentUserService;
        _db = db;
    }

    public bool ShouldBeInvoked(Message message)
    {
        return message.Text!.Trim().StartsWith("/timezone");
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var user = await _db.Users.SingleAsync(e => e.Id == _currentUserService.TelegramUser.Id, cancellationToken);

        if (await ExtractTimeZoneAsync(message, cancellationToken) is not { } timeZone)
            return;

        user.TimeZone = timeZone;
        _db.Update(user);
        await _db.SaveChangesAsync(cancellationToken);

        var formatted = $"{timeZone.Hours}:{timeZone.Minutes:00}";
        await _bot
            .SendTextMessageAsync(
                _currentUserService.TelegramUser.Id,
                $"✅ Установлен часовой пояс {(timeZone > TimeSpan.Zero ? "+" + formatted : formatted)}",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }

    private async Task<TimeSpan?> ExtractTimeZoneAsync(Message message, CancellationToken cancellationToken)
    {
        if (TimeSpan.TryParseExact(
                message.Text!.Trim()["/timezone".Length..].Trim(),
                @"h\:mm",
                CultureInfo.InvariantCulture, out var timeZone))
            return timeZone;

        await _bot
            .SendTextMessageAsync(
                _currentUserService.TelegramUser.Id,
                "/timezone &lt;смещение от UTC в формате 00:00&gt; - Установить свой часовой пояс. Требуется указать смещение от UTC.",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        return null;
    }
}