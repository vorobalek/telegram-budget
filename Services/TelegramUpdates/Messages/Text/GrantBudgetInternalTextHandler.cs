using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class GrantBudgetInternalTextHandler(
    ITelegramBotClient bot,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
    : ITextHandler
{
    public bool ShouldBeInvoked(Message message)
    {
        return message.Text!.Trim().StartsWith("/share_") &&
               message.Text!.Trim()["/share_".Length..].Split('_') is { } args &&
               args.Length == 2 &&
               long.TryParse(args[0], out _) &&
               Guid.TryParse(args[1], out _);
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var args = message.Text!.Trim()["/share_".Length..].Split('_');
        var budgetId = Guid.Parse(args[1]);

        if (await db
                .Budgets
                .FirstOrDefaultAsync(e => e.Id == budgetId, cancellationToken) is not { } budgetToShare)
            return;

        var userToShareId = long.Parse(args[0]);
        var userToShare = await db
            .Users
            .SingleAsync(e => e.Id == userToShareId, cancellationToken);

        if (await db
                .Participating
                .AnyAsync(e =>
                        e.ParticipantId == userToShareId &&
                        e.BudgetId == budgetId,
                    cancellationToken))
        {
            await bot
                .SendTextMessageAsync(
                    currentUserService.TelegramUser.Id,
                    string.Format(TR.L+"ALREADY_GRANTED", userToShare.GetFullNameLink(), budgetToShare.Name.EscapeHtml()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        var participantIds = await db
            .Participating
            .Where(e => e.BudgetId == budgetToShare.Id)
            .Select(e => e.ParticipantId)
            .ToListAsync(cancellationToken);

        var newParticipant = new Participating
        {
            Participant = userToShare,
            Budget = budgetToShare
        };
        await db.Participating.AddAsync(newParticipant, cancellationToken);

        budgetToShare.Participating.Add(newParticipant);
        db.Budgets.Update(budgetToShare);

        userToShare.ActiveBudget ??= budgetToShare;
        db.Users.Update(userToShare);

        await db.SaveChangesAsync(cancellationToken);

        foreach (var participantId in participantIds)
            await bot
                .SendTextMessageAsync(
                    participantId,
                    string.Format(TR.L+"GRANTED_TO_USER", budgetToShare.Name.EscapeHtml(), userToShare.GetFullNameLink(), currentUserService.TelegramUser.GetFullNameLink()),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);

        await bot
            .SendTextMessageAsync(
                userToShare.Id,
                string.Format(TR.L+"GRANTED_TO_YOU", budgetToShare.Name.EscapeHtml(), currentUserService.TelegramUser.GetFullNameLink()),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }
}