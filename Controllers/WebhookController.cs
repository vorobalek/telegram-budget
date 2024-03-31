using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using TelegramBudget.Services.TelegramUpdates;

namespace TelegramBudget.Controllers;

public class WebhookController(IHandleUpdateService handleUpdateService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update)
    {
        await handleUpdateService.HandleUpdateAsync(update, default);
        return Ok();
    }
}