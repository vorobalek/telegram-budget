namespace TelegramBudget.Services.DateTimeProvider;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow();
}