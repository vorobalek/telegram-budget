using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Extensions;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.TelegramBotClientWrapper;

namespace TelegramBudget.Services.TelegramApi.Handle;

internal sealed class RevokePrefixBotCommand(
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
                .FirstOrDefaultAsync(e => e.Id == budgetId, cancellationToken) is not { } budgetToUnShare)
            return;

        var userToUnShare = await db
            .User
            .SingleAsync(e => e.Id == userToShareId, cancellationToken);

        if (await db
                .Participant
                .FirstOrDefaultAsync(e =>
                        e.UserId == userToUnShare.Id &&
                        e.BudgetId == budgetToUnShare.Id,
                    cancellationToken) is not { } participant)
        {
            await botWrapper
                .SendMessage(
                    currentUserService.TelegramUser.Id,
                    string.Format(TR.L + "ALREADY_REVOKED", userToUnShare.GetFullNameLink(),
                        budgetToUnShare.Name.EscapeHtml()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        if (budgetToUnShare.CreatedBy == userToUnShare.Id)
        {
            await botWrapper
                .SendMessage(
                    currentUserService.TelegramUser.Id,
                    string.Format(TR.L + "REVOKING_RESTRICTED", userToUnShare.GetFullNameLink(),
                        budgetToUnShare.Name.EscapeHtml()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        db.Participant.Remove(participant);

        budgetToUnShare.Participating.Remove(participant);
        db.Update(budgetToUnShare);

        userToUnShare.ActiveBudget ??= budgetToUnShare;
        db.Update(userToUnShare);

        await db.SaveChangesAsync(cancellationToken);

        var participantIds = await db
            .Participant
            .Where(e => e.BudgetId == budgetToUnShare.Id)
            .Select(e => e.UserId)
            .ToListAsync(cancellationToken);

        foreach (var participantId in participantIds)
            await botWrapper
                .SendMessage(
                    participantId,
                    string.Format(TR.L + "REVOKED_FOR_USER", budgetToUnShare.Name.EscapeHtml(),
                        userToUnShare.GetFullNameLink(), currentUserService.TelegramUser.GetFullNameLink()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);

        await botWrapper
            .SendMessage(
                userToUnShare.Id,
                string.Format(TR.L + "REVOKED_FOR_YOU", budgetToUnShare.Name.EscapeHtml(),
                    currentUserService.TelegramUser.GetFullNameLink()),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }
}