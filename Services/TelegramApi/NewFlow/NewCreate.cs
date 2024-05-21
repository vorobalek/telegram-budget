using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramApi.UserPrompt;
using TelegramBudget.Services.TelegramBotClientWrapper;
using Tracee;
using User = TelegramBudget.Data.Entities.User;

namespace TelegramBudget.Services.TelegramApi.NewFlow;

internal sealed class NewCreate(
    ITracee tracee,
    ApplicationDbContext db,
    ICurrentUserService currentUserService,
    ITelegramBotWrapper botWrapper) : 
    ICallbackQueryFlow,
    IUserPromptFlow
{
    public const string Command = "create";

    public async Task ProcessAsync(int __, string ___, CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("create");
        
        var user = await GetUserAsync(cancellationToken);
        var message = await SubmitAsync(TR.L + "_CREATE_REQUEST_BUDGET_NAME", true, cancellationToken);
        await UpdateUserAsync(user, message.MessageId, cancellationToken);
    }

    private async Task<User> GetUserAsync(CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("get_user");
        
        return await db.User.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);
    }

    private async Task UpdateUserAsync(User user, int messageId, CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("update_user");
        
        (user.PromptSubject, user.PromptMessageId) = (UserPromptSubjectType.BudgetName, messageId);
        db.Update(user);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Message> SubmitAsync(string text, bool forceReply, CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("submit");

        return await botWrapper
            .SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                text,
                parseMode: ParseMode.Html,
                replyMarkup: forceReply
                    ? new ForceReplyMarkup
                    {
                        InputFieldPlaceholder = TR.L + "_CREATE_REQUEST_BUDGET_NAME_PLACEHOLDER"
                    }
                    : Keyboards.BackToMainInline,
                cancellationToken: cancellationToken);
    }

    public UserPromptSubjectType SubjectType => UserPromptSubjectType.BudgetName;

    public async Task ProcessPromptAsync(User user, Update update, CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("create");
        
        if (!TryGetBudgetName(update, user.PromptMessageId, out var budgetName))
        {
            await ProcessAsync(0, string.Empty, cancellationToken);
            return;
        }

        var text = await PrepareReplyAsync(user, budgetName, cancellationToken);
        await SubmitAsync(text, false, cancellationToken);
    }

    private bool TryGetBudgetName(Update update, int? messageId, out string budgetName)
    {
        if (update is
            {
                Type: UpdateType.Message,
                Message:
                {
                    ReplyToMessage.MessageId: var repliedMessageId,
                    Type: MessageType.Text,
                    Text: { } text,
                }
            } &&
            repliedMessageId == messageId &&
            (update.Message.Entities?.All(e => e.Type != MessageEntityType.BotCommand) ?? true))
        {
            budgetName = text.Trim().Truncate(250).WithFallbackValue();
            return !string.IsNullOrWhiteSpace(budgetName);
        }

        budgetName = string.Empty;
        return false;
    }

    private async Task<string> PrepareReplyAsync(User user, string budgetName, CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("prepare");
        
        if (await BudgetAlreadyExistsAsync(budgetName, cancellationToken))
        {
            await ProcessAsync(0, string.Empty, cancellationToken);
            return string.Format(TR.L + "ALREADY_EXISTS", budgetName.EscapeHtml());
        }

        await CreateNewBudgetAsync(user, budgetName, cancellationToken);
        return string.Format(TR.L + "CREATED", budgetName.EscapeHtml());
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