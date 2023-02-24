using Common.Database;
using Common.Database.Services;
using Common.Database.Traits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using TelegramBudget.Services;

namespace TelegramBudget.Data.Entities;

public class Budget :
    Entity<Budget>,
    IIdTrait<Guid>,
    ICreatedByTrait<long?>
{
    private ICollection<User> _activeUsers;

    private User? _owner;

    private ICollection<Participating> _participating;

    private ICollection<Transaction> _transactions;
    public string Name { get; set; } = null!;

    [JsonIgnore]
    public ICollection<User> ActiveUsers
    {
        get => Lazy(ref _activeUsers);
        set => _activeUsers = value;
    }

    [JsonIgnore]
    public ICollection<Transaction> Transactions
    {
        get => Lazy(ref _transactions);
        set => _transactions = value;
    }

    [JsonIgnore]
    public ICollection<Participating> Participating
    {
        get => Lazy(ref _participating);
        set => _participating = value;
    }

    [JsonIgnore]
    public User? Owner
    {
        get => Lazy(ref _owner);
        set => _owner = value;
    }

    public long? CreatedBy { get; set; }

    public Guid Id { get; set; }

    public sealed class ChangeListener : EntityChangeListener<Budget>
    {
        private readonly ICurrentUserService _currentUserService;

        public ChangeListener(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        protected override void OnModelCreating(EntityTypeBuilder<Budget> builder)
        {
            base.OnModelCreating(builder);

            builder
                .HasIndex(e => new { e.Name, e.CreatedBy })
                .IsUnique();

            builder
                .HasOne(e => e.Owner)
                .WithMany(e => e.OwnedBudgets)
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            builder
                .Property(e => e.Name)
                .HasMaxLength(250);
        }
    }
}