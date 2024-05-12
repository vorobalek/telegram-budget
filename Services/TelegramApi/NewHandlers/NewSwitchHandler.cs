using System.Data;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBudget.Data;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;
using Tracee;
using User = TelegramBudget.Data.Entities.User;

namespace TelegramBudget.Services.TelegramApi.NewHandlers;

public class NewSwitchHandler(
    ApplicationDbContext db,
    ICurrentUserService currentUserService,
    ITelegramBotClientWrapper botWrapper,
    ILogger<NewSwitchHandler> logger,
    ITracee tracee) : ICallbackQueryHandler
{
    public const string Command = "switch";
    public const string CommandPrefix = "switch.";

    public async Task ProcessAsync(
        int messageId,
        string data,
        CancellationToken cancellationToken)
    {
        using var trace = tracee.Scoped("switch");
        var budgetId = ParseArguments(trace, data);

        var (text, availableBudgets) = await PrepareReplyAsync(
            trace,
            budgetId,
            cancellationToken);

        await SubmitReplyAsync(
            trace,
            messageId,
            text,
            availableBudgets,
            cancellationToken);
    }

    private Guid? ParseArguments(
        ITracee trace,
        string data)
    {
        using var scope = trace.Scoped("parse");
        var arguments = data.Split('.').Skip(1).ToArray();

        if (arguments.Length < 1) return null;

        Guid? budgetId = Guid.TryParse(arguments[0], out var budgetGuid) ? budgetGuid : null;
        return budgetId;
    }

    private async Task<(
        string Text,
        ICollection<(Guid Id, string Name, decimal Sum)>? AvailableBudgets)
    > PrepareReplyAsync(
        ITracee trace,
        Guid? budgetId,
        CancellationToken cancellationToken)
    {
        using var scope = trace.Scoped("prepare");
        var user = await GetUserAsync(scope, cancellationToken);

        if (budgetId.HasValue &&
            await GetBudgetNameAsync(scope, budgetId.Value, cancellationToken) is { } budgetName &&
            await TrySetActiveBudgetAsync(scope, user, budgetId.Value, cancellationToken))
        {
            var activeBudgetText = string.Format(TR.L + "_SWITCH_ACTIVE_BUDGET", budgetName);

            var ownerInfo = await GetBudgetOwnerInfoAsync(scope, budgetId.Value, cancellationToken);
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

        var availableBudgets = await GetAvailableBudgetsAsync(scope, user.ActiveBudgetId, cancellationToken);
        return (TR.L + "_SWITCH_CHOOSE_BUDGET", availableBudgets);
    }

    private async Task<(long? Id, string? Name)> GetBudgetOwnerInfoAsync(
        ITracee trace,
        Guid budgetId,
        CancellationToken cancellationToken)
    {
        using var scope = trace.Scoped("get_budget_owner");
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

    private async Task<User> GetUserAsync(ITracee trace, CancellationToken cancellationToken)
    {
        using var scope = trace.Scoped("get_user");
        return await db.Users.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);
    }

    private async Task<string?> GetBudgetNameAsync(ITracee trace, Guid budgetId, CancellationToken cancellationToken)
    {
        using var scope = trace.Scoped("get_budget");
        return await db.Budgets
            .Where(e => e.Id == budgetId)
            .Select(e => e.Name)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task<bool> TrySetActiveBudgetAsync(
        ITracee trace,
        User user,
        Guid budgetId,
        CancellationToken cancellationToken)
    {
        using var scope = trace.Scoped("set_budget");
        await using var transaction = await db.Database
            .BeginTransactionAsync(IsolationLevel.Snapshot, cancellationToken);
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
        ITracee trace,
        Guid? activeBudgetId,
        CancellationToken cancellationToken)
    {
        using var scope = trace.Scoped("get_budget_list");
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

    private async Task SubmitReplyAsync(ITracee trace,
        int messageId,
        string text,
        ICollection<(Guid Id, string Name, decimal Sum)>? availableBudgets,
        CancellationToken cancellationToken)
    {
        using var scope = trace.Scoped("submit");
        var keyboard = GetKeyboard(scope, availableBudgets);

        await botWrapper
            .EditMessageTextAsync(
                currentUserService.TelegramUser.Id,
                messageId,
                text,
                ParseMode.Html,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
    }

    private static InlineKeyboardMarkup GetKeyboard(
        ITracee trace,
        ICollection<(Guid Id, string Name, decimal Sum)>? availableBudgets)
    {
        using var scope = trace.Scoped("get_keyboard");
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