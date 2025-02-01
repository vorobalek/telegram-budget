using TelegramBudget.Services.CurrentUser;

namespace TelegramBudget.Middleware;

internal sealed class ExceptionHandlerMiddleware(ICurrentUserService currentUserService) : IMiddleware
{
    public async Task InvokeAsync(
        HttpContext context,
        RequestDelegate next)
    {
        context.Request.EnableBuffering();
        using var streamReader = new StreamReader(
            context.Request.Body,
            leaveOpen: true);
        var body = await streamReader.ReadToEndAsync();
        context.Request.Body.Position = 0;
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex, scope =>
            {
                scope.Contexts[nameof(ICurrentUserService.TelegramUser)] =
                    (object?)currentUserService.TryGetTelegramUser() ?? "undefined";
                scope.Contexts[nameof(HttpRequest)] = new
                {
                    Headers = context.Request.Headers.ToDictionary(
                        x => x.Key,
                        x => x.Value.ToString()),
                    Body = body
                };
            });
        }
    }
}