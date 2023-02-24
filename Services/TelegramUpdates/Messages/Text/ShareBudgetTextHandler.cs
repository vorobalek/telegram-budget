using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;
using User = TelegramBudget.Data.Entities.User;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class ShareBudgetTextHandler : ITextHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _db;

    public ShareBudgetTextHandler(
        ITelegramBotClient bot,
        ICurrentUserService currentUserService,
        ApplicationDbContext db)
    {
        _bot = bot;
        _currentUserService = currentUserService;
        _db = db;
    }

    public bool ShouldBeInvoked(Message message)
    {
        return message.Text!.Trim().StartsWith("/share") &&
               !message.Text!.Trim().StartsWith("/share_");
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var user = await _db.Users.SingleAsync(e => e.Id == _currentUserService.TelegramUser.Id, cancellationToken);
        if (await ExtractArgumentsAsync(message, user, cancellationToken) is not { } args)
            return;

        if (await _db
                .Participating
                .AnyAsync(e =>
                        e.ParticipantId == args.UserToShare.Id &&
                        e.BudgetId == args.BudgetToShare.Id,
                    cancellationToken))
        {
            await _bot
                .SendTextMessageAsync(
                    _currentUserService.TelegramUser.Id,
                    $"❌ Пользователь {args.UserToShare.GetFullNameLink()} уже имеет доступ к бюджету с именем &quot;{args.BudgetToShare.Name.EscapeHtml()}&quot;",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        var participantIds = await _db
            .Participating
            .Where(e => e.BudgetId == args.BudgetToShare.Id)
            .Select(e => e.ParticipantId)
            .ToListAsync(cancellationToken);

        var newParticipant = new Participating
        {
            Participant = args.UserToShare,
            Budget = args.BudgetToShare
        };
        await _db.Participating.AddAsync(newParticipant, cancellationToken);

        args.BudgetToShare.Participating.Add(newParticipant);
        _db.Budgets.Update(args.BudgetToShare);

        args.UserToShare.ActiveBudget ??= args.BudgetToShare;
        _db.Users.Update(args.UserToShare);

        await _db.SaveChangesAsync(cancellationToken);

        foreach (var participantId in participantIds)
            await _bot
                .SendTextMessageAsync(
                    participantId,
                    $"✅ Доступ к бюдету с именем &quot;{args.BudgetToShare.Name.EscapeHtml()}&quot; предоставлен {args.UserToShare.GetFullNameLink()}" +
                    Environment.NewLine +
                    Environment.NewLine +
                    $"<i>Инициатор: {_currentUserService.TelegramUser.GetFullNameLink()}</i>",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);

        await _bot
            .SendTextMessageAsync(
                args.UserToShare.Id,
                $"❗ Вам предоставлен доступ к бюджету &quot;{args.BudgetToShare.Name.EscapeHtml()}&quot; пользователем {_currentUserService.TelegramUser.GetFullNameLink()}",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
    }

    private async Task<(User UserToShare, Budget BudgetToShare)?> ExtractArgumentsAsync(Message message, User user,
        CancellationToken cancellationToken)
    {
        var userToShareIdString = message.Text!.Trim()["/share".Length..].Trim().Split()[0];
        if (long.TryParse(userToShareIdString, out var userToShareId))
        {
            if (await _db.Users.FirstOrDefaultAsync(e => e.Id == userToShareId, cancellationToken) is { } userToShare)
            {
                var budgetName = message.Text!.Trim()["/share".Length..].Trim()[userToShareIdString.Length..].Trim();
                if (!string.IsNullOrWhiteSpace(budgetName))
                {
                    if (await _db
                            .Budgets
                            .Where(e => e.Name == budgetName)
                            .ToListAsync(cancellationToken) is { Count: > 0 } budgets)
                    {
                        if (budgets.Count == 1) return (userToShare, budgets[0]);

                        await budgets.SendPaginatedAsync(
                            (pageBuilder, pageNumber) =>
                            {
                                pageBuilder.AppendLine(
                                    $"❌ <b>Доступно несколько бюджетов с именем &quot;{budgetName.EscapeHtml()}&quot;</b> <i>(страница {pageNumber})</i>");
                                pageBuilder.AppendLine();
                                pageBuilder.AppendLine(
                                    $"<i>Выберите, к которому вы хотите предоставить доступ пользователю {userToShare.GetFullNameLink()} " +
                                    $"и кликните на соответствующую ему команду, она отправится боту.</i>");
                                pageBuilder.AppendLine();
                            },
                            budget =>
                            {
                                return $"{budget.Name.EscapeHtml()} " +
                                       (budget.Owner is not { } owner
                                           ? "<i>(владелец неизвестен)</i> "
                                           : owner.Id == _currentUserService.TelegramUser.Id
                                               ? "<i>(владелец – вы)</i> "
                                               : $"<i>(владелец – {budget.Owner.GetFullNameLink()})</i> ") +
                                       " ➡️ " +
                                       $"/share_{userToShareId}_{budget.Id:N}";
                            },
                            (pageBuilder, currentString) =>
                            {
                                pageBuilder.AppendLine();
                                pageBuilder.AppendLine(currentString);
                            },
                            async (pageContent, token) =>
                                await _bot
                                    .SendTextMessageAsync(
                                        _currentUserService.TelegramUser.Id,
                                        pageContent,
                                        parseMode: ParseMode.Html,
                                        cancellationToken: token),
                            4096,
                            cancellationToken);
                        return null;
                    }

                    await _bot
                        .SendTextMessageAsync(
                            _currentUserService.TelegramUser.Id,
                            $"❌ Не найден бюджет с именем &quot;{budgetName.EscapeHtml()}&quot;",
                            parseMode: ParseMode.Html,
                            cancellationToken: cancellationToken);
                    return null;
                }

                if (user.ActiveBudget is { } activeBudget)
                    return (userToShare, activeBudget);

                await _bot
                    .SendTextMessageAsync(
                        _currentUserService.TelegramUser.Id,
                        "❌ У вас не выбран активный бюджет. Установите его командой /switch",
                        parseMode: ParseMode.Html,
                        cancellationToken: cancellationToken);
                return null;
            }

            await _bot
                .SendTextMessageAsync(
                    _currentUserService.TelegramUser.Id,
                    $"❌ Не найден пользователь с номером {userToShareId}.",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return null;
        }

        await _bot
            .SendTextMessageAsync(
                _currentUserService.TelegramUser.Id,
                $"❌ Недопустимый номер пользователя &quot;{userToShareIdString.EscapeHtml()}&quot;. – смотрите /help.",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        return null;
    }
}