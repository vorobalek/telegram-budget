using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;
using User = TelegramBudget.Data.Entities.User;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class GrantBudgetTextHandler(
    ITelegramBotClient bot,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
    : ITextHandler
{
    public bool ShouldBeInvoked(Message message)
    {
        return message.Text!.Trim().StartsWith("/grant") &&
               !message.Text!.Trim().StartsWith("/share_");
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleAsync(e => e.Id == currentUserService.TelegramUser.Id, cancellationToken);
        if (await ExtractArgumentsAsync(message, user, cancellationToken) is not { } args)
            return;

        if (await db
                .Participating
                .AnyAsync(e =>
                        e.ParticipantId == args.UserToShare.Id &&
                        e.BudgetId == args.BudgetToShare.Id,
                    cancellationToken))
        {
            await bot
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    string.Format(TR.L+"ALREADY_GRANTED", args.UserToShare.GetFullNameLink(), args.BudgetToShare.Name.EscapeHtml()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        var participantIds = await db
            .Participating
            .Where(e => e.BudgetId == args.BudgetToShare.Id)
            .Select(e => e.ParticipantId)
            .ToListAsync(cancellationToken);

        var newParticipant = new Participating
        {
            Participant = args.UserToShare,
            Budget = args.BudgetToShare
        };
        await db.Participating.AddAsync(newParticipant, cancellationToken);

        args.BudgetToShare.Participating.Add(newParticipant);
        db.Budgets.Update(args.BudgetToShare);

        args.UserToShare.ActiveBudget ??= args.BudgetToShare;
        db.Users.Update(args.UserToShare);

        await db.SaveChangesAsync(cancellationToken);

        foreach (var participantId in participantIds)
            await bot
                .SendTextMessageAsync(
                    participantId,
                    string.Format(TR.L+"GRANTED_TO_USER", args.BudgetToShare.Name.EscapeHtml(), args.UserToShare.GetFullNameLink(), currentUserService.TelegramUser.GetFullNameLink()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);

        await bot
            .SendTextMessageAsync(
                args.UserToShare.Id,
                string.Format(TR.L+"GRANTED_TO_YOU", args.BudgetToShare.Name.EscapeHtml(), currentUserService.TelegramUser.GetFullNameLink()),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }

    private async Task<(User UserToShare, Budget BudgetToShare)?> ExtractArgumentsAsync(Message message, User user,
        CancellationToken cancellationToken)
    {
        var userToShareIdString = message.Text!.Trim()["/grant".Length..].Trim().Split()[0];

        if (string.IsNullOrWhiteSpace(userToShareIdString))
        {
            await bot
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    (TR.L+"GRANT").EscapeHtml(),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return null;
        }
        
        if (long.TryParse(userToShareIdString, out var userToShareId))
        {
            if (await db.Users.FirstOrDefaultAsync(e => e.Id == userToShareId, cancellationToken) is { } userToShare)
            {
                var budgetName = message.Text!.Trim()["/grant".Length..].Trim()[userToShareIdString.Length..].Trim();
                if (!string.IsNullOrWhiteSpace(budgetName))
                {
                    if (await db
                            .Budgets
                            .Where(e => e.Name == budgetName)
                            .ToListAsync(cancellationToken) is { Count: > 0 } budgets)
                    {
                        if (budgets.Count == 1) return (userToShare, budgets[0]);

                        await budgets.SendPaginatedAsync(
                            (pageBuilder, pageNumber) =>
                            {
                                pageBuilder.Append(string.Format(TR.L+"CHOOSE_BUDGET_GRANT", budgetName.EscapeHtml(), pageNumber, userToShare.GetFullNameLink()));
                            },
                            budget =>
                            {
                                return $"{budget.Name.EscapeHtml()} " +
                                       "<i>(" +
                                       (budget.Owner is not { } owner
                                           ? TR.L+"OWNER_UNKNOWN"
                                           : owner.Id == currentUserService.TelegramUser.Id
                                               ? TR.L+"OWNER_YOU"
                                               : string.Format(TR.L+"OWNER_USER", budget.Owner.GetFullNameLink())) +
                                       ")</i>" +
                                       " ➡️ " +
                                       $"/share_{userToShareId}_{budget.Id:N}";
                            },
                            (pageBuilder, currentString) =>
                            {
                                pageBuilder.AppendLine();
                                pageBuilder.AppendLine(currentString);
                            },
                            async (pageContent, token) =>
                                await bot
                                    .SendTextMessageAsync(
                                        currentUserService.TelegramUser.Id,
                                        pageContent,
                                        parseMode: ParseMode.Html,
                                        cancellationToken: token),
                            4096,
                            cancellationToken);
                        return null;
                    }

                    await bot
                        .SendTextMessageAsync(
                            currentUserService.TelegramUser.Id,
                            string.Format(TR.L+"BUDGET_NOT_FOUND", budgetName.EscapeHtml()),
                            parseMode: ParseMode.Html,
                            cancellationToken: cancellationToken);
                    return null;
                }

                if (user.ActiveBudget is { } activeBudget)
                    return (userToShare, activeBudget);

                await bot
                    .SendTextMessageAsync(
                        currentUserService.TelegramUser.Id,
                        TR.L+"NO_ACTIVE_BUDGET",
                        parseMode: ParseMode.Html,
                        cancellationToken: cancellationToken);
                return null;
            }

            await bot
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    string.Format(TR.L+"USER_NOT_FOUND", userToShareId),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return null;
        }

        await bot
            .SendTextMessageAsync(
                currentUserService.TelegramUser.Id,
                string.Format(TR.L+"INVALID_USER_ID", userToShareIdString.EscapeHtml()),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        return null;
    }
}