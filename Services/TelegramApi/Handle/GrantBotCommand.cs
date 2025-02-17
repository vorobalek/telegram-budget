using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handle;

internal sealed class GrantBotCommand(
    ITelegramBotWrapper botWrapper,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
{
    public async Task ProcessAsync(string data, CancellationToken cancellationToken)
    {
        var user = await db.User.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);
        if (await ExtractArgumentsAsync(data, user, cancellationToken) is not { } args)
            return;

        if (await db
                .Participant
                .AnyAsync(e =>
                        e.UserId == args.UserToShare.Id &&
                        e.BudgetId == args.BudgetToShare.Id,
                    cancellationToken))
        {
            await botWrapper
                .SendMessage(
                    currentUserService.TelegramUser.Id,
                    string.Format(TR.L + "ALREADY_GRANTED", args.UserToShare.GetFullNameLink(),
                        args.BudgetToShare.Name.EscapeHtml()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        var participantIds = await db
            .Participant
            .Where(e => e.BudgetId == args.BudgetToShare.Id)
            .Select(e => e.UserId)
            .ToListAsync(cancellationToken);

        var newParticipant = new Participant
        {
            User = args.UserToShare,
            Budget = args.BudgetToShare
        };
        await db.Participant.AddAsync(newParticipant, cancellationToken);

        args.BudgetToShare.Participating.Add(newParticipant);
        db.Update(args.BudgetToShare);

        args.UserToShare.ActiveBudget ??= args.BudgetToShare;
        db.Update(args.UserToShare);

        await db.SaveChangesAsync(cancellationToken);

        foreach (var participantId in participantIds)
            await botWrapper
                .SendMessage(
                    participantId,
                    string.Format(TR.L + "GRANTED_TO_USER", args.BudgetToShare.Name.EscapeHtml(),
                        args.UserToShare.GetFullNameLink(), currentUserService.TelegramUser.GetFullNameLink()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);

        await botWrapper
            .SendMessage(
                args.UserToShare.Id,
                string.Format(TR.L + "GRANTED_TO_YOU", args.BudgetToShare.Name.EscapeHtml(),
                    currentUserService.TelegramUser.GetFullNameLink()),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }

    private async Task<(User UserToShare, Budget BudgetToShare)?> ExtractArgumentsAsync(string data, User user,
        CancellationToken cancellationToken)
    {
        var userToShareIdString = data.Split()[0];

        if (string.IsNullOrWhiteSpace(userToShareIdString))
        {
            await botWrapper
                .SendMessage(
                    currentUserService.TelegramUser.Id,
                    TR.L + "GRANT_EXAMPLE",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return null;
        }

        if (!long.TryParse(userToShareIdString, out var userToShareId))
        {
            await botWrapper
                .SendMessage(
                    currentUserService.TelegramUser.Id,
                    string.Format(TR.L + "INVALID_USER_ID", userToShareIdString.EscapeHtml()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return null;
        }

        if (await db.User.FirstOrDefaultAsync(e => e.Id == userToShareId, cancellationToken) is not { } userToShare)
        {
            await botWrapper
                .SendMessage(
                    currentUserService.TelegramUser.Id,
                    string.Format(TR.L + "USER_NOT_FOUND", userToShareId),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return null;
        }

        var budgetName = data[userToShareIdString.Length..].Trim();
        if (string.IsNullOrWhiteSpace(budgetName))
        {
            if (user.ActiveBudget is { } activeBudget)
                return (userToShare, activeBudget);

            await botWrapper
                .SendMessage(
                    currentUserService.TelegramUser.Id,
                    TR.L + "NO_ACTIVE_BUDGET",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return null;
        }

        if (await db
                .Budget
                .Where(e => e.Name == budgetName)
                .ToListAsync(cancellationToken) is not { Count: > 0 } budgets)
        {
            await botWrapper
                .SendMessage(
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
                    pageBuilder.Append(string.Format(TR.L + "CHOOSE_BUDGET_GRANT", budgetName.EscapeHtml(),
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
                           $"/grant_{userToShareId}_{budget.Id:N}";
                }, (pageBuilder, currentString) =>
                {
                    pageBuilder.AppendLine();
                    pageBuilder.AppendLine(currentString);
                }, async (pageContent, token) =>
                    await botWrapper
                        .SendMessage(
                            currentUserService.TelegramUser.Id,
                            pageContent,
                            parseMode: ParseMode.Html,
                            cancellationToken: token), cancellationToken);
            return null;
        }

        return (userToShare, budgets[0]);
    }
}