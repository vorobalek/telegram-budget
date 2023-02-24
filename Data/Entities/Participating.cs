using Common.Database;
using Common.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace TelegramBudget.Data.Entities;

public class Participating :
    Entity<Participating>
{
    private Budget _budget;
    private User _participant;
    public long ParticipantId { get; set; }

    [JsonIgnore]
    public User Participant
    {
        get => Lazy(ref _participant);
        set => _participant = value;
    }

    public Guid BudgetId { get; set; }

    [JsonIgnore]
    public Budget Budget
    {
        get => Lazy(ref _budget);
        set => _budget = value;
    }

    public sealed class ChangeListener : EntityChangeListener<Participating>
    {
        protected override void OnModelCreating(EntityTypeBuilder<Participating> builder)
        {
            base.OnModelCreating(builder);

            builder
                .HasKey(e => new { e.ParticipantId, e.BudgetId });

            builder
                .HasOne(e => e.Participant)
                .WithMany(e => e.Participating)
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(e => e.Budget)
                .WithMany(e => e.Participating)
                .HasForeignKey(e => e.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}