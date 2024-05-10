using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using TelegramBudget.Extensions;
using TelegramBudget.Services;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramApi;

namespace TelegramBudget.Controllers;

public class WebhookController(
    ITelegramApiService telegramApiService,
    ICurrentUserService currentUserService,
    GlobalCancellationTokenSource cancellationTokenSource,
    ILogger<WebhookController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update)
    {
        var sw = Stopwatch.StartNew();
        currentUserService.TelegramUser = update.GetUser();
        await telegramApiService.HandleUpdateAsync(update, cancellationTokenSource.Token);
        sw.Stop();
        logger.LogDebug("Request finished in {Milliseconds} milliseconds", sw.ElapsedMilliseconds);
        return Ok();
    }
}