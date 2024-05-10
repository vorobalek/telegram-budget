using TelegramBudget.Services.Trace;

namespace TelegramBudget.Middleware.Trace;

internal sealed class TraceMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext httpContext,
        ITraceService trace)
    {
        using (trace.Create("request"))
        {
            await next(httpContext);
        }
        trace.LogDebugAll();
    }
}