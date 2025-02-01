using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using Telegram.Flow.Updates.Messages.Texts.BotCommands;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramApi.NewFlow.Infrastructure;
using TelegramBudget.Services.TelegramBotClientWrapper;
using Tracee;

namespace TelegramBudget.Services.TelegramApi.NewFlow;

internal sealed class CancelFlow(
    ITracee tracee,
    ApplicationDbContext db,
    ICurrentUserService currentUserService,
    ITelegramBotWrapper botWrapper,
    MainFlow mainFlow) : IBotCommandFlow
{
    public const string Command = "cancel";
    
    public async Task ProcessAsync(IBotCommandContext context, CancellationToken cancellationToken)
    {
        await ProcessAsync(cancellationToken);
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("cancel");

        var user = await GetUserAsync(cancellationToken);
        var text = PrepareReply(user.PromptSubject);
        await UpdateUserAsync(user, cancellationToken);
        await Task.WhenAll(
            SubmitReplyAsync(text, cancellationToken),
            mainFlow.ProcessAsync(cancellationToken));
    }

    private async Task<User> GetUserAsync(CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("get_user");
        
        return await db.User.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);
    }

    private string PrepareReply(
        UserPromptSubjectType? promptSubject)
    {
        return promptSubject switch
        {
            null => TR.L + "_CANCEL_NOTHING",
            UserPromptSubjectType.BudgetName => TR.L + "_CANCEL_DONE_BUDGET_NAME",
            _ => throw new ArgumentOutOfRangeException(nameof(promptSubject), promptSubject, null)
        };
    }

    private async Task UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("update_user");
        
        (user.PromptSubject, user.PromptMessageId) = (null, null);
        db.Update(user);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SubmitReplyAsync(
        string text,
        CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("submit");
        
        await botWrapper
            .SendMessage(
                currentUserService.TelegramUser.Id,
                text,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }
}