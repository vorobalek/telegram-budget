using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using TelegramBudget.Extensions;
using TelegramBudget.Services;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramApi;
using TelegramBudget.Services.Trace;

namespace TelegramBudget.Controllers;

public class WebhookController(
    ITelegramApiService telegramApiService,
    ICurrentUserService currentUserService,
    GlobalCancellationTokenSource cancellationTokenSource,
    ITraceService trace) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update)
    {
        currentUserService.TelegramUser = update.GetUser();
        await telegramApiService.HandleUpdateAsync(update, cancellationTokenSource.Token);
        return Ok();
    }
}