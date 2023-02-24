using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Extensions;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class ListBudgetTextHandler : ITextHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _db;

    public ListBudgetTextHandler(
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
        return message.Text!.Trim().StartsWith("/list");
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var user = await _db.Users.SingleAsync(e => e.Id == _currentUserService.TelegramUser.Id, cancellationToken);
        var budgets = await _db
            .Budgets
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.Owner,
                Sum = e.Transactions.Sum(t => t.Amount)
            })
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);

        if (!budgets.Any())
        {
            await _bot
                .SendTextMessageAsync(
                    _currentUserService.TelegramUser.Id,
                    "❌ У вас пока нет бюджетов. Создайте новый, используя команду /create",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        await budgets.SendPaginatedAsync(
            (pageBuilder, pageNumber) =>
            {
                pageBuilder.AppendLine($"✅ <b>Список доступных бюджетов</b> <i>(страница {pageNumber})</i>");
                pageBuilder.AppendLine();
                pageBuilder.AppendLine("<i>Жирным шрифтом отмечен активный активный бюджет.</i>");
                pageBuilder.AppendLine();
            },
            budget =>
            {
                var (@in, @out) = user.ActiveBudgetId == budget.Id
                    ? ("<b>", "</b>")
                    : (string.Empty, string.Empty);

                return $"{@in}{budget.Name.EscapeHtml()}{@out} " +
                       $"➡️ {@in}{budget.Sum:0.00}{@out} " +
                       (budget.Owner is not { } owner
                           ? "<i>(владелец неизвестен)</i> "
                           : owner.Id == _currentUserService.TelegramUser.Id
                               ? "<i>(владелец – вы)</i> "
                               : $"<i>(владелец – {budget.Owner.GetFullNameLink()})</i> ");
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
    }
}