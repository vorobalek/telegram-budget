using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Configuration;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.DateTimeProvider;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handle;

internal sealed class TimezoneBotCommand(
    ITelegramBotWrapper botWrapper,
    ICurrentUserService currentUserService,
    ApplicationDbContext db,
    IDateTimeProvider dateTime)
{
    public async Task ProcessAsync(string data, CancellationToken cancellationToken)
    {
        var responseMessageBuilder = new StringBuilder();
        var user = await db.User.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);

        if (ExtractTimeZoneAsync(data) is not { } timeZone)
        {
            responseMessageBuilder.Clear();
            responseMessageBuilder.AppendLine(
                string.Format(
                    TR.L + "TIMEZONE_CURRENT",
                    GetUserTimeZoneFormatted(user)));
            responseMessageBuilder.AppendLine();
            responseMessageBuilder.AppendLine(
                string.Format(
                    TR.L + "TIME_CURRENT",
                    TR.L + dateTime.UtcNow().DateTime.Add(user.TimeZone) + AppConfiguration.DateTimeFormat));
            responseMessageBuilder.AppendLine();
            responseMessageBuilder.AppendLine(TR.L + "TIMEZONE_EXAMPLE");

            await botWrapper
                .SendMessage(
                    currentUserService.TelegramUser.Id,
                    responseMessageBuilder.ToString(),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        user.TimeZone = timeZone;
        db.Update(user);
        await db.SaveChangesAsync(cancellationToken);

        responseMessageBuilder.Clear();
        responseMessageBuilder.AppendLine(
            string.Format(
                TR.L + "TIMEZONE_SET",
                GetUserTimeZoneFormatted(user)));
        responseMessageBuilder.AppendLine();
        responseMessageBuilder.AppendLine(
            string.Format(
                TR.L + "TIME_CURRENT",
                TR.L + dateTime.UtcNow().DateTime.Add(user.TimeZone) + AppConfiguration.DateTimeFormat));

        await botWrapper
            .SendMessage(
                currentUserService.TelegramUser.Id,
                responseMessageBuilder.ToString(),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }

    private static string GetUserTimeZoneFormatted(User user)
    {
        var formatted = $"{Math.Abs(user.TimeZone.Hours):00}:{Math.Abs(user.TimeZone.Minutes):00}";
        return user.TimeZone > TimeSpan.Zero ? "+" + formatted : "-" + formatted;
    }

    private static TimeSpan? ExtractTimeZoneAsync(string data)
    {
        if (data.StartsWith('-'))
            return TimeSpan.TryParseExact(
                data,
                @"\-h\:mm",
                CultureInfo.InvariantCulture, out var negativeTimeZone)
                ? -negativeTimeZone
                : null;
        if (data.StartsWith('+'))
            return TimeSpan.TryParseExact(
                data,
                @"\+h\:mm",
                CultureInfo.InvariantCulture, out var positiveTimeZone)
                ? +positiveTimeZone
                : null;

        return TimeSpan.TryParseExact(
            data,
            @"h\:mm",
            CultureInfo.InvariantCulture, out var timeZone)
            ? timeZone
            : null;
    }
}