using TelegramBudget.Configuration;

namespace TelegramBudget.Middleware;

public class SecretTokenValidatorMiddleware
{
    private const string SecretTokenHeader = "X-Telegram-Bot-Api-Secret-Token";
    private readonly RequestDelegate _next;

    public SecretTokenValidatorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var secretToken = context.Request.Headers
            .FirstOrDefault(x => x.Key == SecretTokenHeader)
            .Value
            .ToString();

        if (string.IsNullOrWhiteSpace(secretToken) ||
            TelegramBotConfiguration.WebhookSecretToken != secretToken)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.CompleteAsync();
            return;
        }

        await _next(context);
    }
}