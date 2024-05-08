using TelegramBudget.Configuration;
using TelegramBudget.Services.DateTimeProvider;

namespace TelegramBudget.Middleware.RequestTime;

internal sealed class RequestTimeMiddleware(RequestDelegate next)
{
    public async Task Invoke(
        HttpContext httpContext,
        IDateTimeProvider dateTimeProvider)
    {
        var requestStartedUtc = dateTimeProvider.UtcNow();
        httpContext.Items.Add(AppConfiguration.RequestStartedOnContextItemName, requestStartedUtc);
        await next(httpContext);
    }
}