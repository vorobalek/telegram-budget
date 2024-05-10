using TelegramBudget.Configuration;
using TelegramBudget.Services.Trace;

namespace TelegramBudget.Middleware;

public class SecretTokenValidatorMiddleware(RequestDelegate next)
{
    private const string SecretTokenHeader = "X-Telegram-Bot-Api-Secret-Token";

    public async Task InvokeAsync(
        HttpContext context,
        ITraceService trace)
    {
        using (trace.Create("secret_token_validation"))
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
}