using Common.Database.Infrastructure;
using Common.Database.Infrastructure.Extensions;
using Common.Database.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TelegramBudget.Data.Entities;
using TelegramBudget.Services.CurrentUser;
using Tracee;

namespace TelegramBudget.Data;

public sealed partial class ApplicationDbContext : DbContext, ICommonDbContext<ApplicationDbContext>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ITracee _tracee;

    public ApplicationDbContext(
        IEntityChangeListenerService<ApplicationDbContext> entityChangeListenerService,
        ICurrentUserService currentUserService,
        DbContextOptions<ApplicationDbContext> options,
        ITracee tracee) : base(options)
    {
        EntityChangeListenerService = entityChangeListenerService;
        _currentUserService = currentUserService;
        _tracee = tracee;
        this.SubscribeCommonDbContext();
    }

    public IEntityChangeListenerService<ApplicationDbContext> EntityChangeListenerService { get; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // optionsBuilder
        //     .UseNpgsql(builder =>
        //     {
        //         builder.MigrationsHistoryTable("__ef_migrations");
        //     })
        //     .UseSnakeCaseNamingConvention();
        base.OnConfiguring(optionsBuilder);
        this.CommonConfiguring(optionsBuilder);
#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();

        void LogSensitive(string data)
        {
            _tracee.Logger.LogTrace("DB_SENSITIVE_LOG: {Data}", data);
        }

        optionsBuilder.LogTo(LogSensitive);
#endif
        optionsBuilder.ConfigureWarnings(builder => { builder.Ignore(CoreEventId.DetachedLazyLoadingWarning); });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        this.CommonModelCreating(modelBuilder);

        modelBuilder
            .Entity<Budget>()
            .HasQueryFilter(e => e
                .Participating
                .Any(p => p
                    .UserId == _currentUserService.TelegramUser.Id));
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        using var _ = _tracee.Scoped("save_db");
        
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override void Dispose()
    {
        base.Dispose();
        _tracee.Dispose();
    }
}