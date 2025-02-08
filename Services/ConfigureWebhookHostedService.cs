using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Configuration;

namespace TelegramBudget.Services;

internal sealed class ConfigureWebhookHostedService(
    ILogger<ConfigureWebhookHostedService> logger,
    IServiceScopeFactory serviceScopeFactory)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        var webhookAddress = string.IsNullOrWhiteSpace(AppConfiguration.PathBase)
            ? $"{AppConfiguration.Url}/bot"
            : $"{AppConfiguration.Url}{AppConfiguration.PathBase}/bot";
        logger.LogInformation("Setting webhook: {WebhookAddress}", webhookAddress);
        await botClient.SetWebhook(
            webhookAddress,
            secretToken: TelegramBotConfiguration.WebhookSecretToken,
            allowedUpdates:
            [
                UpdateType.Message,
                UpdateType.EditedMessage,
                UpdateType.CallbackQuery
            ],
            cancellationToken: cancellationToken);
        await botClient
            .SetMyCommands(
                TelegramBotConfiguration.DeclaredCommands,
                cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        // Remove webhook upon app shutdown
        logger.LogInformation("Removing webhook");
        await botClient.DeleteWebhook(cancellationToken: cancellationToken);
    }
}