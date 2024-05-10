using System.Data;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBudget.Data;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;
using User = TelegramBudget.Data.Entities.User;

namespace TelegramBudget.Services.TelegramApi.NewHandlers;

public class NewSwitchHandler(
    ApplicationDbContext db,
    ICurrentUserService currentUserService,
    ITelegramBotClientWrapper botWrapper,
    ILogger<NewSwitchHandler> logger) : ICallbackQueryHandler
{
    public const string Command = "switch";
    public const string CommandPrefix = "switch.";

    public async Task ProcessAsync(
        int messageId,
        string data,
        CancellationToken cancellationToken)
    {
        var budgetId = ParseArguments(data);

        var (text, availableBudgets) = await PrepareReplyAsync(budgetId, cancellationToken);

        await SubmitReplyAsync(
            messageId,
            text,
            availableBudgets,
            cancellationToken);
    }

    private Guid? ParseArguments(string data)
    {
        var arguments = data.Split('.').Skip(1).ToArray();

        if (arguments.Length < 1) return null;

        Guid? budgetId = Guid.TryParse(arguments[0], out var budgetGuid) ? budgetGuid : null;
        return budgetId;
    }

    private async Task<(
        string Text, 
        ICollection<(Guid Id, string Name, decimal Sum)>? AvailableBudgets)
    > PrepareReplyAsync(Guid? budgetId, CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(cancellationToken);

        if (budgetId.HasValue &&
            await GetBudgetNameAsync(budgetId.Value, cancellationToken) is { } budgetName &&
            await TrySetActiveBudgetAsync(user, budgetId.Value, cancellationToken))
        {
            var activeBudgetText = string.Format(TR.L + "_SWITCH_ACTIVE_BUDGET", budgetName);
            
            var ownerInfo = await GetBudgetOwnerInfoAsync(budgetId.Value, cancellationToken);
            var ownerText = ownerInfo.Id switch
            {
                not null when ownerInfo.Id == user.Id => 
                    TR.L + "_OWNER_YOU",
                not null when ownerInfo.Name is not null => 
                    string.Format(TR.L + "_OWNER_USER", ownerInfo.Id, ownerInfo.Name),
                _ => 
                    TR.L + "_OWNER_UNKNOWN"
            };
            
            return ($"{activeBudgetText}{ownerText}", null);
        }
        
        var availableBudgets = await GetAvailableBudgetsAsync(user.ActiveBudgetId, cancellationToken);
        return (TR.L + "_SWITCH_CHOOSE_BUDGET", availableBudgets);
    }

    private async Task<(long? Id, string? Name)> GetBudgetOwnerInfoAsync(Guid budgetId, CancellationToken cancellationToken)
    {
        var data = await db.Budgets
            .Where(e => e.Id == budgetId)
            .Select(e => new
            {
                Id = e.CreatedBy,
                FirstName = e.Owner == null ? null : e.Owner.FirstName,
                LastName = e.Owner == null ? null : e.Owner.LastName
            })
            .SingleAsync(cancellationToken);

        return (data.Id, data.FirstName is null ? null : TelegramHelper.GetFullName(data.FirstName, data.LastName));
    }

    private Task<User> GetUserAsync(CancellationToken cancellationToken)
    {
        return db.Users.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);
    }

    private Task<string?> GetBudgetNameAsync(Guid budgetId, CancellationToken cancellationToken)
    {
        return db.Budgets
            .Where(e => e.Id == budgetId)
            .Select(e => e.Name)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task<bool> TrySetActiveBudgetAsync(User user, Guid budgetId, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Snapshot, cancellationToken);
        try
        {
            user.ActiveBudgetId = budgetId;
            db.Users.Update(user);

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(exception, "An error while updating user's active budget");
        }

        return false;
    }

    private async Task<ICollection<(Guid Id, string Name, decimal Sum)>> GetAvailableBudgetsAsync(
        Guid? activeBudgetId,
        CancellationToken cancellationToken)
    {
        var data = await db.Budgets
            .Where(e => e.Id != activeBudgetId)
            .Select(e => new
            {
                e.Id,
                e.Name,
                Sum = e.Transactions.Select(transaction => transaction.Amount).Sum()
            })
            .ToArrayAsync(cancellationToken);

        return data.Select(e => (e.Id, e.Name, e.Sum)).ToArray();
    }

    private Task<Message> SubmitReplyAsync(
        int messageId,
        string text,
        ICollection<(Guid Id, string Name, decimal Sum)>? availableBudgets,
        CancellationToken cancellationToken)
    {
        var keyboard = GetKeyboard(availableBudgets);

        return botWrapper
            .EditMessageTextAsync(
                currentUserService.TelegramUser.Id,
                messageId,
                text,
                ParseMode.Html,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
    }

    private static InlineKeyboardMarkup GetKeyboard(ICollection<(Guid Id, string Name, decimal Sum)>? availableBudgets)
    {
        return new InlineKeyboardMarkup(
            availableBudgets != null
                ? new List<IEnumerable<InlineKeyboardButton>>(
                        [[Keyboards.CreateBudgetInlineButton]])
                    .Concat(
                        Keyboards.BuildSelectItemInlineButtons(
                            availableBudgets,
                            item => 
                                string.Format(
                                    TR.L + "_SWITCH_CHOOSE_BUDGET_BTN",
                                    item.Name.Truncate(32),
                                    item.Sum),
                            item => $"{CommandPrefix}{item.Id:N}"))
                    .Concat(
                        [[Keyboards.BackToMainInlineButton]])
                : [[Keyboards.BackToMainInlineButton]]);
    }
}