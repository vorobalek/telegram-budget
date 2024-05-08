namespace TelegramBudget.Middleware.RequestTime;

internal static class RequestTimeMiddlewareExtensions
{
    internal static IApplicationBuilder UseRequestTimestamp(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestTimeMiddleware>();
    }
}