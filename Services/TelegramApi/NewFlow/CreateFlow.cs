using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Flow.Updates.CallbackQueries.Data;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramApi.NewFlow.Infrastructure;
using TelegramBudget.Services.TelegramApi.UserPrompt;
using TelegramBudget.Services.TelegramBotClientWrapper;
using Tracee;
using User = TelegramBudget.Data.Entities.User;

namespace TelegramBudget.Services.TelegramApi.NewFlow;

internal sealed class CreateFlow(
    ITracee tracee,
    ApplicationDbContext db,
    ICurrentUserService currentUserService,
    ITelegramBotWrapper botWrapper,
    MainFlow mainFlow) : 
    ICallbackQueryFlow,
    IUserPromptFlow
{
    public const string Command = "create";

    public async Task ProcessAsync(IDataContext context, CancellationToken cancellationToken)
    {
        await ProcessAsync(cancellationToken);
    }

    private async Task ProcessAsync(CancellationToken cancellationToken, string? text = null)
    {
        using var _ = tracee.Scoped("create");
        
        var user = await GetUserAsync(cancellationToken);
        text ??= $"{TR.L + "_CREATE_REQUEST_BUDGET_NAME_HEADER"}{TR.L + "_CREATE_REQUEST_BUDGET_NAME"}";
        var message = await SubmitAsync(text, true, cancellationToken);
        await UpdateUserAsync(user, message.MessageId, cancellationToken);
    }

    private async Task<User> GetUserAsync(CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("get_user");
        
        return await db.User.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);
    }

    private async Task<Message> SubmitAsync(string text, bool forceReply, CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("submit");

        return await botWrapper
            .SendMessage(
                currentUserService.TelegramUser.Id,
                text,
                parseMode: ParseMode.Html,
                replyMarkup: forceReply
                    ? new ForceReplyMarkup
                    {
                        InputFieldPlaceholder = TR.L + "_CREATE_REQUEST_BUDGET_NAME_PLACEHOLDER"
                    }
                    : null,
                cancellationToken: cancellationToken);
    }

    private async Task UpdateUserAsync(User user, int messageId, CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("update_user");
        
        (user.PromptSubject, user.PromptMessageId) = (UserPromptSubjectType.BudgetName, messageId);
        db.Update(user);
        await db.SaveChangesAsync(cancellationToken);
    }

    public UserPromptSubjectType SubjectType => UserPromptSubjectType.BudgetName;

    public async Task ProcessPromptAsync(User user, Update update, CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("create");
        
        if (!TryGetBudgetName(update, out var budgetName))
        {
            await ProcessAsync(cancellationToken);
            return;
        }
        
        if (await BudgetAlreadyExistsAsync(budgetName, cancellationToken))
        {
            await ProcessAsync(
                cancellationToken,
                $"{string.Format(
                    TR.L + "_CREATE_REQUEST_BUDGET_NAME_ALREADY_EXISTS",
                    budgetName.EscapeHtml())}{TR.L + "_CREATE_REQUEST_BUDGET_NAME"}");
            return;
        }

        await CreateNewBudgetAsync(user, budgetName, cancellationToken);
        await SubmitAsync(
            string.Format(
                TR.L + "CREATED",
                budgetName.EscapeHtml()),
            false,
            cancellationToken);
        await mainFlow.ProcessAsync(cancellationToken);
    }

    private bool TryGetBudgetName(Update update, out string budgetName)
    {
        if (update is
            {
                Type: UpdateType.Message,
                Message:
                {
                    Type: MessageType.Text,
                    Text: { } text,
                }
            } &&
            (update.Message.Entities?.All(e => e.Type != MessageEntityType.BotCommand) ?? true))
        {
            budgetName = text.Trim().Truncate(250).WithFallbackValue();
            return !string.IsNullOrWhiteSpace(budgetName);
        }

        budgetName = string.Empty;
        return false;
    }

    private async Task<bool> BudgetAlreadyExistsAsync(
        string budgetName,
        CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("exists");
        
        return await db.Budget.AnyAsync(e => e.Name == budgetName, cancellationToken);
    }

    private async Task CreateNewBudgetAsync(User user, string budgetName, CancellationToken cancellationToken)
    {
        using var __ = tracee.Scoped("new");

        var newBudget = new Budget
        {
            Name = budgetName,
            ActiveUsers = [user]
        };
        await db.Budget.AddAsync(newBudget, cancellationToken);

        var participating = new Participant
        {
            Budget = newBudget,
            User = user
        };
        await db.Participant.AddAsync(participating, cancellationToken);

        user.ActiveBudget = newBudget;
        (user.PromptSubject, user.PromptMessageId) = (null, null);
        db.Update(user);

        await db.SaveChangesAsync(cancellationToken);
    }
}