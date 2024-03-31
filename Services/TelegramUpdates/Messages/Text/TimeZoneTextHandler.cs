using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Extensions;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class TimeZoneTextHandler(
    ITelegramBotClient bot,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
    : ITextHandler
{
    public bool ShouldBeInvoked(Message message)
    {
        return message.Text!.Trim().StartsWith("/timezone");
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);

        if (await ExtractTimeZoneAsync(message, cancellationToken) is not { } timeZone)
            return;

        user.TimeZone = timeZone;
        db.Update(user);
        await db.SaveChangesAsync(cancellationToken);

        var formatted = $"{timeZone.Hours}:{timeZone.Minutes:00}";
        await bot
            .SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                string.Format(TR.L+"TZ_SET", timeZone > TimeSpan.Zero ? "+" + formatted : formatted),
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

        await bot
            .SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                (TR.L+"TZ").EscapeHtml(),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        return null;
    }
}