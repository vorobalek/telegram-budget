using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using TelegramBudget.Services.TelegramUpdates;

namespace TelegramBudget.Controllers;

public class WebhookController : ControllerBase
{
    private readonly IHandleUpdateService _handleUpdateService;

    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        ILogger<WebhookController> logger,
        IHandleUpdateService handleUpdateService)
    {
        _logger = logger;
        _handleUpdateService = handleUpdateService;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update)
    {
        try
        {
            await _handleUpdateService.HandleUpdateAsync(update, default);
        }
        catch (Exception exception)
        {
            _logger.LogCritical(exception, "Unexpected error while processing telegram webhook");
        }

        return Ok();
    }
}