using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBudget.Data;
using TelegramBudget.Data.Entities;
using TelegramBudget.Extensions;

namespace TelegramBudget.Services.TelegramUpdates.Messages.Text;

public class TransactionTextHandler : ITextHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _db;

    public TransactionTextHandler(
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
        return decimal.TryParse(message.Text!.Trim().Split()[0], out _);
    }

    public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
    {
        var user = await _db.Users.SingleAsync(e => e.Id == _currentUserService.TelegramUser.Id, cancellationToken);

        if (user.ActiveBudget is null)
        {
            await _bot
                .SendTextMessageAsync(
                    _currentUserService.TelegramUser.Id,
                    "❌ У вас не выбран активный бюджет. Установите его командой /switch",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            return;
        }

        var rawAmount = message.Text!.Trim().Split()[0];
        var amount = decimal.Parse(rawAmount);
        var oldBudgetSum = user.ActiveBudget.Transactions.Sum(e => e.Amount);

        var newTransaction = new Transaction
        {
            Amount = amount,
            BudgetId = user.ActiveBudget.Id,
            MessageId = message.MessageId,
            Comment = message.Text!.Trim().Length > rawAmount.Length
                ? message.Text!.Trim()[rawAmount.Length..].Trim().Truncate(250)
                : null
        };

        await _db.Transactions.AddAsync(newTransaction, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var newBudgetSum = user.ActiveBudget.Transactions.Sum(e => e.Amount);

        var needSaveChange = false;
        foreach (var participating in user.ActiveBudget.Participating)
        {
            var confirmationMessage = await _bot
                .SendTextMessageAsync(
                    participating.ParticipantId,
                    $"💰 <b>{user.ActiveBudget.Name.EscapeHtml()}</b> 💰" +
                    Environment.NewLine +
                    Environment.NewLine +
                    $"{oldBudgetSum:0.00} " +
                    $"<b>{(amount >= 0 ? "➕ " + amount.ToString("0.00") : "➖ " + Math.Abs(amount).ToString("0.00"))}</b> " +
                    $"➡️ {newBudgetSum:0.00}" +
                    (newTransaction.Comment is not null
                        ? Environment.NewLine +
                          Environment.NewLine +
                          newTransaction.Comment.EscapeHtml()
                        : string.Empty) +
                    Environment.NewLine +
                    Environment.NewLine +
                    $"<i>добавлено {(user.TimeZone == TimeSpan.Zero ? newTransaction.CreatedAt.ToString("dd.MM.yyyy HH:mm") + " UTC" : newTransaction.CreatedAt.Add(user.TimeZone).ToString("dd.MM.yyyy HH:mm"))} " +
                    $"{_currentUserService.TelegramUser.GetFullNameLink()}</i>",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            await _db.TransactionConfirmations.AddAsync(new TransactionConfirmation
            {
                MessageId = confirmationMessage.MessageId,
                RecipientId = participating.ParticipantId,
                Transaction = newTransaction
            }, cancellationToken);
            needSaveChange = true;
        }

        if (needSaveChange)
            await _db.SaveChangesAsync(cancellationToken);
    }
}