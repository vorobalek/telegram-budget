using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Configuration;

namespace TelegramBudget.Services;

public class ConfigureWebhook(
    ILogger<ConfigureWebhook> logger,
    IServiceScopeFactory serviceScopeFactory)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        var webhookAddress = $"{AppConfiguration.Url}/bot";
        logger.LogInformation("Setting webhook: {WebhookAddress}", webhookAddress);
        await botClient.SetWebhookAsync(
            webhookAddress,
            secretToken: TelegramBotConfiguration.WebhookSecretToken,
            allowedUpdates: new[]
            {
                UpdateType.Message,
                UpdateType.EditedMessage
            },
            cancellationToken: cancellationToken);
        await botClient
            .SetMyCommandsAsync(TelegramBotConfiguration.Commands, cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        // Remove webhook upon app shutdown
        logger.LogInformation("Removing webhook");
        await botClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
    }
}