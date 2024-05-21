using System.ComponentModel.DataAnnotations.Schema;
using Common.Database;
using Common.Database.Hosts;
using Common.Database.Services;
using Common.Database.Traits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace TelegramBudget.Data.Entities;

public sealed class Transaction :
    Entity<Transaction>,
    IIdTrait<Guid>,
    ICreatedAtTrait,
    ICreatedByTrait<long?>,
    IVersionHost<Transaction, Guid, Transaction.Version>
{
    private User? _author;
    private Budget _budget;
    private ICollection<TransactionConfirmation> _confirmations;

    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string? Comment { get; set; }
    public int MessageId { get; set; }
    public Guid BudgetId { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }

    [JsonIgnore]
    public User Author
    {
        get => Lazy(ref _author);
        set => _author = value;
    }

    [JsonIgnore]
    public Budget Budget
    {
        get => Lazy(ref _budget);
        set => _budget = value;
    }

    [JsonIgnore]
    public ICollection<TransactionConfirmation> Confirmations
    {
        get => Lazy(ref _confirmations);
        set => _confirmations = value;
    }

    //[Table("transaction_version")]
    public sealed class Version : EntityVersion<Transaction, Guid, Version>;

    public sealed class ChangeListener : EntityChangeListener<Transaction>
    {
        protected override void OnModelCreating(EntityTypeBuilder<Transaction> builder)
        {
            base.OnModelCreating(builder);

            builder
                .HasOne(e => e.Author)
                .WithMany(e => e.Transactions)
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            builder
                .HasOne(e => e.Budget)
                .WithMany(e => e.Transactions)
                .HasForeignKey(e => e.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasIndex(e => new { e.CreatedBy, e.MessageId })
                .IsUnique();

            builder
                .Property(e => e.Comment)
                .HasMaxLength(250);
        }
    }
}