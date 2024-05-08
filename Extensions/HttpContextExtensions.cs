using TelegramBudget.Configuration;
using TelegramBudget.Services.DateTimeProvider;

namespace TelegramBudget.Extensions;

internal static class HttpContextExtensions
{
    private static DateTimeOffset? TryGetRequestStartedDateTimeOffset(this HttpContext httpContext)
    {
        if (httpContext.Items.ContainsKey(AppConfiguration.RequestStartedOnContextItemName))
            if (httpContext.Items[AppConfiguration.RequestStartedOnContextItemName] is DateTimeOffset stamp)
                return stamp;

        return null;
    }

    internal static double? TryGetRequestTimeMs(this HttpContext httpContext)
    {
        if (httpContext.TryGetRequestStartedDateTimeOffset() is not { } stamp) return null;

        var dateTimeProvider = httpContext.RequestServices.GetRequiredService<IDateTimeProvider>();
        return (dateTimeProvider.UtcNow() - stamp).TotalMilliseconds;
    }
}