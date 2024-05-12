using TelegramBudget.Configuration;

namespace TelegramBudget.Middleware;

public class SecretTokenValidatorMiddleware : IMiddleware
{
    private const string SecretTokenHeader = "X-Telegram-Bot-Api-Secret-Token";

    public async Task InvokeAsync(
        HttpContext context,
        RequestDelegate next)
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

        await next(context);
    }
}