namespace TelegramBudget.Services.DateTimeProvider;

internal interface IDateTimeProvider
{
    DateTimeOffset UtcNow();
}