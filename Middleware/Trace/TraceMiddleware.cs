using TelegramBudget.Services.Trace;

namespace TelegramBudget.Middleware.Trace;

internal sealed class TraceMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext httpContext,
        ITraceService trace)
    {
        using (trace.Scope("request_time"))
        {
            await next(httpContext);
        }
        trace.LogSynced(LogLevel.Debug);
    }
}