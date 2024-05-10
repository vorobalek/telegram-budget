namespace TelegramBudget.Middleware.Trace;

internal static class TraceMiddlewareExtensions
{
    internal static IApplicationBuilder UseTrace(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TraceMiddleware>();
    }
}