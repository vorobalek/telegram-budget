using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Flow.Updates.CallbackQueries.Data;
using TelegramBudget.Data;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;
using Tracee;
using User = TelegramBudget.Data.Entities.User;

namespace TelegramBudget.Services.TelegramApi.NewFlow;

internal sealed class NewSwitch(
    ITracee tracee,
    ApplicationDbContext db,
    ICurrentUserService currentUserService,
    ITelegramBotWrapper botWrapper,
    NewMain mainFlow) : ICallbackQueryFlow
{
    public const string Command = "switch";
    public const string CommandPrefix = "switch.";

    public async Task ProcessAsync(
        IDataContext context,
        CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("switch");
        
        var budgetId = ParseArguments(context.Data);

        var (text, availableBudgets) = await PrepareReplyAsync(
            budgetId,
            cancellationToken);

        await SubmitReplyAsync(
            context.CallbackQuery.Message!.MessageId,
            text,
            availableBudgets,
            cancellationToken);
    }

    private Guid? ParseArguments(
        string data)
    {
        var arguments = data.Split('.').Skip(1).ToArray();

        if (arguments.Length < 1) return null;

        Guid? budgetId = Guid.TryParse(arguments[0], out var budgetGuid) ? budgetGuid : null;
        return budgetId;
    }

    private async Task<(
        string Text,
        ICollection<(Guid Id, string Name, decimal Sum)>? AvailableBudgets)
    > PrepareReplyAsync(
        Guid? budgetId,
        CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("prepare");
        
        var user = await GetUserAsync(cancellationToken);

        if (budgetId.HasValue &&
            await GetBudgetNameAsync(budgetId.Value, cancellationToken) is { } budgetName)
        {
            await SetActiveBudgetAsync(user, budgetId.Value, cancellationToken);
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

    private async Task<(long? Id, string? Name)> GetBudgetOwnerInfoAsync(
        Guid budgetId,
        CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("get_owner");
        
        var data = await db.Budget
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

    private async Task<User> GetUserAsync(CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("get_user");
        
        return await db.User.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);
    }

    private async Task<string?> GetBudgetNameAsync(Guid budgetId, CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("get_name");
        
        return await db.Budget
            .Where(e => e.Id == budgetId)
            .Select(e => e.Name)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task SetActiveBudgetAsync(
        User user,
        Guid budgetId,
        CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("update_user");
        
        user.ActiveBudgetId = budgetId;
        db.Update(user);

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<ICollection<(Guid Id, string Name, decimal Sum)>> GetAvailableBudgetsAsync(
        Guid? activeBudgetId,
        CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("get_list");
        
        var data = await db.Budget
            .Where(e => e.Id != activeBudgetId)
            .Select(e => new
            {
                e.Id,
                e.Name,
                Sum = e.Transactions.Select(transaction => transaction.Amount).Sum()
            })
            .OrderBy(e => e.Name)
            .ToArrayAsync(cancellationToken);

        return data.Select(e => (e.Id, e.Name, e.Sum)).ToArray();
    }

    private async Task SubmitReplyAsync(
        int messageId,
        string text,
        ICollection<(Guid Id, string Name, decimal Sum)>? availableBudgets,
        CancellationToken cancellationToken)
    {
        using var _ = tracee.Scoped("submit");

        if (availableBudgets is null)
        {
            await Task.WhenAll(
                botWrapper
                    .EditMessageTextAsync(
                        currentUserService.TelegramUser.Id,
                        messageId,
                        text,
                        parseMode: ParseMode.Html,
                        cancellationToken: cancellationToken),
                mainFlow
                    .ProcessAsync(
                        cancellationToken));
            return;
        }

        var keyboard = GetKeyboard(availableBudgets);

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
        ICollection<(Guid Id, string Name, decimal Sum)> availableBudgets)
    {
        var buttons = Keyboards.BuildSelectItemInlineButtons(
            availableBudgets,
            item =>
                string.Format(
                    TR.L + "_BTN_SWITCH_CHOOSE_BUDGET",
                    item.Name.Truncate(32),
                    item.Sum),
            item => $"{CommandPrefix}{item.Id:N}");
        if (availableBudgets.Count < 19)
            buttons = buttons.Concat([[Keyboards.CreateInlineButton]]);
        buttons = buttons.Concat([[Keyboards.BackToMainInlineButton]]);

        return new InlineKeyboardMarkup(buttons);
    }
}