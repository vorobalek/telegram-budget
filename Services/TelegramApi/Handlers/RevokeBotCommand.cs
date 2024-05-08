using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handlers;

public sealed class RevokeBotCommand(
    ITelegramBotClientWrapper bot,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
{
    public async Task ProcessAsync(string data, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);
        if (await ExtractArgumentsAsync(data, user, cancellationToken) is not { } args)
            return;

        if (await db
                .Participating
                .FirstOrDefaultAsync(e =>
                        e.ParticipantId == args.UserToUnShare.Id &&
                        e.BudgetId == args.BudgetToUnShare.Id,
                    cancellationToken) is not { } participant)
        {
            await bot
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    string.Format(TR.L + "ALREADY_REVOKED", args.UserToUnShare.GetFullNameLink(),
                        args.BudgetToUnShare.Name.EscapeHtml()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        if (args.BudgetToUnShare.CreatedBy == args.UserToUnShare.Id)
        {
            await bot
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    string.Format(TR.L + "REVOKING_RESTRICTED", args.UserToUnShare.GetFullNameLink(),
                        args.BudgetToUnShare.Name.EscapeHtml()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        db.Participating.Remove(participant);

        args.BudgetToUnShare.Participating.Remove(participant);
        db.Budgets.Update(args.BudgetToUnShare);

        args.UserToUnShare.ActiveBudget ??= args.BudgetToUnShare;
        db.Users.Update(args.UserToUnShare);

        await db.SaveChangesAsync(cancellationToken);

        var participantIds = await db
            .Participating
            .Where(e => e.BudgetId == args.BudgetToUnShare.Id)
            .Select(e => e.ParticipantId)
            .ToListAsync(cancellationToken);

        foreach (var participantId in participantIds)
            await bot
                .SendTextMessageAsync(
                    participantId,
                    string.Format(TR.L + "REVOKED_FOR_USER", args.BudgetToUnShare.Name.EscapeHtml(),
                        args.UserToUnShare.GetFullNameLink(), currentUserService.TelegramUser.GetFullNameLink()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);

        await bot
            .SendTextMessageAsync(
                args.UserToUnShare.Id,
                string.Format(TR.L + "REVOKED_FOR_YOU", args.BudgetToUnShare.Name.EscapeHtml(),
                    currentUserService.TelegramUser.GetFullNameLink()),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }
    
    private async Task<(User UserToUnShare, Budget BudgetToUnShare)?> ExtractArgumentsAsync(
        string data, 
        User user,
        CancellationToken cancellationToken)
    {
        var userToUnShareIdString = data.Split()[0];

        if (string.IsNullOrWhiteSpace(userToUnShareIdString))
        {
            await bot
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    TR.L + "REVOKE_EXAMPLE",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return null;
        }

        if (!long.TryParse(userToUnShareIdString, out var userToShareId))
        {
            await bot
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    string.Format(TR.L + "INVALID_USER_ID", userToUnShareIdString.EscapeHtml()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return null;
        }

        if (await db.Users.FirstOrDefaultAsync(e => e.Id == userToShareId, cancellationToken) is not { } userToShare)
        {
            await bot
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    string.Format(TR.L + "USER_NOT_FOUND", userToShareId),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return null;
        }

        var budgetName = data[userToUnShareIdString.Length..].Trim();
        if (string.IsNullOrWhiteSpace(budgetName))
        {
            if (user.ActiveBudget is { } activeBudget)
                return (userToShare, activeBudget);

            await bot
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    TR.L + "NO_ACTIVE_BUDGET",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return null;
        }

        if (await db
                .Budgets
                .Where(e => e.Name == budgetName)
                .ToListAsync(cancellationToken) is not { Count: > 0 } budgets)
        {
            await bot
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    string.Format(TR.L + "BUDGET_NOT_FOUND", budgetName.EscapeHtml()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return null;
        }

        if (budgets.Count != 1)
        {
            await budgets.SendPaginatedAsync(
                4096,
                (pageBuilder, pageNumber) =>
                {
                    pageBuilder.Append(string.Format(TR.L + "CHOOSE_BUDGET_REVOKE", budgetName.EscapeHtml(),
                        pageNumber, userToShare.GetFullNameLink()));
                }, budget =>
                {
                    return $"{budget.Name.EscapeHtml()} " +
                           "<i>(" +
                           (budget.Owner is not { } owner
                               ? TR.L + "OWNER_UNKNOWN"
                               : owner.Id == currentUserService.TelegramUser.Id
                                   ? TR.L + "OWNER_YOU"
                                   : string.Format(TR.L + "OWNER_USER", budget.Owner.GetFullNameLink())) +
                           ")</i>" +
                           " ➡️ " +
                           $"/revoke_{userToShareId}_{budget.Id:N}";
                }, (pageBuilder, currentString) =>
                {
                    pageBuilder.AppendLine();
                    pageBuilder.AppendLine(currentString);
                }, async (pageContent, token) =>
                    await bot
                        .SendTextMessageAsync(
                            currentUserService.TelegramUser.Id,
                            pageContent,
                            parseMode: ParseMode.Html,
                            cancellationToken: token), cancellationToken);
            return null;
        }

        return (userToShare, budgets[0]);
    }
}