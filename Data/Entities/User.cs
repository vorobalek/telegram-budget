using Common.Database;
using Common.Database.Services;
using Common.Database.Traits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using TelegramBudget.Extensions;

namespace TelegramBudget.Data.Entities;

public sealed class User :
    Entity<User>,
    IIdTrait<long>
{
    private Budget? _activeBudget;
    private ICollection<Budget> _ownedBudgets;
    private ICollection<Participant> _participating;
    private ICollection<TransactionConfirmation> _transactionConfirmations;
    private ICollection<Transaction> _transactions;

    // Telegram Id
    public long Id { get; set; }

    public string FirstName { get; set; }
    public string? LastName { get; set; }
    public TimeSpan TimeZone { get; set; }

    public int? PromptMessageId { get; set; }
    public UserPromptSubjectType? PromptSubject { get; set; }

    [JsonIgnore]
    public ICollection<Transaction> Transactions
    {
        get => Lazy(ref _transactions);
        set => _transactions = value;
    }

    public Guid? ActiveBudgetId { get; set; }

    [JsonIgnore]
    public Budget? ActiveBudget
    {
        get => Lazy(ref _activeBudget);
        set => _activeBudget = value;
    }

    [JsonIgnore]
    public ICollection<Participant> Participating
    {
        get => Lazy(ref _participating);
        set => _participating = value;
    }

    [JsonIgnore]
    public ICollection<TransactionConfirmation> TransactionConfirmations
    {
        get => Lazy(ref _transactionConfirmations);
        set => _transactionConfirmations = value;
    }

    [JsonIgnore]
    public ICollection<Budget> OwnedBudgets
    {
        get => Lazy(ref _ownedBudgets);
        set => _ownedBudgets = value;
    }

    public string GetFullNameLink()
    {
        return TelegramHelper.GetFullNameLink(Id, FirstName, LastName);
    }

    public sealed class ChangeListener : EntityChangeListener<User>
    {
        protected override void OnModelCreating(EntityTypeBuilder<User> builder)
        {
            base.OnModelCreating(builder);

            builder
                .Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.FirstName);
            builder.Property(e => e.LastName);

            builder
                .Property(e => e.TimeZone)
                .HasDefaultValue(TimeSpan.Zero);

            builder
                .HasOne(e => e.ActiveBudget)
                .WithMany(e => e.ActiveUsers)
                .HasForeignKey(e => e.ActiveBudgetId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}