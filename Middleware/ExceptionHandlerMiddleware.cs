using TelegramBudget.Services.CurrentUser;

namespace TelegramBudget.Middleware;

public class ExceptionHandlerMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ICurrentUserService currentUserService)
    {
        context.Request.EnableBuffering();
        using var streamReader = new StreamReader(context.Request.Body);
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