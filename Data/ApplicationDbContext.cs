using Common.Database.Infrastructure;
using Common.Database.Infrastructure.Extensions;
using Common.Database.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TelegramBudget.Data.Entities;
using TelegramBudget.Services.CurrentUser;

namespace TelegramBudget.Data;

public partial class ApplicationDbContext : DbContext, ICommonDbContext<ApplicationDbContext>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ApplicationDbContext> _logger;

    public ApplicationDbContext(
        IEntityChangeListenerService<ApplicationDbContext> entityChangeListenerService,
        ICurrentUserService currentUserService,
        DbContextOptions<ApplicationDbContext> options,
        ILogger<ApplicationDbContext> logger) : base(options)
    {
        EntityChangeListenerService = entityChangeListenerService;
        _currentUserService = currentUserService;
        _logger = logger;
        this.SubscribeCommonDbContext();
    }

    public IEntityChangeListenerService<ApplicationDbContext> EntityChangeListenerService { get; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        this.CommonConfiguring(optionsBuilder);
#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();

        void LogSensitive(string data)
        {
            _logger.LogTrace("DB_SENSITIVE_LOG: {Data}", data);
        }

        optionsBuilder.LogTo(LogSensitive);
#endif
        optionsBuilder.ConfigureWarnings(builder => { builder.Ignore(CoreEventId.DetachedLazyLoadingWarning); });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        this.CommonModelCreating(modelBuilder);

        modelBuilder.Entity<Budget>().HasQueryFilter(e =>
            e.Participating
                .Any(p =>
                    p.ParticipantId == _currentUserService.TelegramUser.Id));
    }
}