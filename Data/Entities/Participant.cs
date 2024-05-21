using Common.Database;
using Common.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace TelegramBudget.Data.Entities;

public sealed class Participant :
    Entity<Participant>
{
    private User _participant;
    private Budget _budget;

    public long UserId { get; set; }
    public Guid BudgetId { get; set; }

    [JsonIgnore]
    public User User
    {
        get => Lazy(ref _participant);
        set => _participant = value;
    }

    [JsonIgnore]
    public Budget Budget
    {
        get => Lazy(ref _budget);
        set => _budget = value;
    }

    public sealed class ChangeListener : EntityChangeListener<Participant>
    {
        protected override void OnModelCreating(EntityTypeBuilder<Participant> builder)
        {
            base.OnModelCreating(builder);

            builder
                .HasKey(e => new { ParticipantId = e.UserId, e.BudgetId });

            builder
                .HasOne(e => e.User)
                .WithMany(e => e.Participating)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(e => e.Budget)
                .WithMany(e => e.Participating)
                .HasForeignKey(e => e.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}