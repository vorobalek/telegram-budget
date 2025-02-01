using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handle;

internal sealed class GrantPrefixBotCommand(
    ITelegramBotWrapper botWrapper,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
{
    public async Task ProcessAsync(string botCommandPostfix, CancellationToken cancellationToken)
    {
        var args = botCommandPostfix.Split('_');

        if (args.Length != 2 ||
            !long.TryParse(args[0], out var userToShareId) ||
            !Guid.TryParse(args[1], out var budgetId))
            return;

        if (await db
                .Budget
                .FirstOrDefaultAsync(e => e.Id == budgetId, cancellationToken) is not { } budgetToShare)
            return;

        var userToShare = await db
            .User
            .SingleAsync(e => e.Id == userToShareId, cancellationToken);

        if (await db
                .Participant
                .AnyAsync(e =>
                        e.UserId == userToShareId &&
                        e.BudgetId == budgetId,
                    cancellationToken))
        {
            await botWrapper
                .SendMessage(
                    currentUserService.TelegramUser.Id,
                    string.Format(TR.L + "ALREADY_GRANTED", userToShare.GetFullNameLink(),
                        budgetToShare.Name.EscapeHtml()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        var participantIds = await db
            .Participant
            .Where(e => e.BudgetId == budgetToShare.Id)
            .Select(e => e.UserId)
            .ToListAsync(cancellationToken);

        var newParticipant = new Participant
        {
            User = userToShare,
            Budget = budgetToShare
        };
        await db.Participant.AddAsync(newParticipant, cancellationToken);

        budgetToShare.Participating.Add(newParticipant);
        db.Update(budgetToShare);

        userToShare.ActiveBudget ??= budgetToShare;
        db.Update(userToShare);

        await db.SaveChangesAsync(cancellationToken);

        foreach (var participantId in participantIds)
            await botWrapper
                .SendMessage(
                    participantId,
                    string.Format(TR.L + "GRANTED_TO_USER", budgetToShare.Name.EscapeHtml(),
                        userToShare.GetFullNameLink(), currentUserService.TelegramUser.GetFullNameLink()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);

        await botWrapper
            .SendMessage(
                userToShare.Id,
                string.Format(TR.L + "GRANTED_TO_YOU", budgetToShare.Name.EscapeHtml(),
                    currentUserService.TelegramUser.GetFullNameLink()),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }
}