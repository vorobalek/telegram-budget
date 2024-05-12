using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using TelegramBudget.Extensions;
using TelegramBudget.Services;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramApi;
using Tracee;

namespace TelegramBudget.Controllers;

public class WebhookController(
    ITelegramApiService telegramApiService,
    ICurrentUserService currentUserService,
    GlobalCancellationTokenSource cancellationTokenSource,
    ITracee tracee) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update)
    {
        using (tracee.Scoped("controller"))
        {
            currentUserService.TelegramUser = update.GetUser();
            await telegramApiService.HandleUpdateAsync(update, cancellationTokenSource.Token);
            return Ok();
        }
    }
}