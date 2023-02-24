using Common.Database;
using Common.Database.Services;
using Common.Database.Traits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace TelegramBudget.Data.Entities;

public class TransactionConfirmation :
    Entity<TransactionConfirmation>,
    IIdTrait<Guid>,
    ICreatedAtTrait
{
    private User _recipient;

    private Transaction _transaction;

    public Guid TransactionId { get; set; }

    [JsonIgnore]
    public Transaction Transaction
    {
        get => Lazy(ref _transaction);
        set => _transaction = value;
    }

    public long RecipientId { get; set; }

    [JsonIgnore]
    public User Recipient
    {
        get => Lazy(ref _recipient);
        set => _recipient = value;
    }

    public int MessageId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid Id { get; set; }

    public sealed class ChangeListener : EntityChangeListener<TransactionConfirmation>
    {
        protected override void OnModelCreating(EntityTypeBuilder<TransactionConfirmation> builder)
        {
            base.OnModelCreating(builder);

            builder
                .HasOne(e => e.Transaction)
                .WithMany(e => e.Confirmations)
                .HasForeignKey(e => e.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(e => e.Recipient)
                .WithMany(e => e.TransactionConfirmations)
                .HasForeignKey(e => e.RecipientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasIndex(e => new { e.MessageId, e.RecipientId })
                .IsUnique();
        }
    }
}