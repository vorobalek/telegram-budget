using Common.Database.Infrastructure;
using Common.Database.Infrastructure.Extensions;
using Common.Database.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TelegramBudget.Data.Entities;
using TelegramBudget.Services.CurrentUser;

namespace TelegramBudget.Data;

public partial class ApplicationDbContext(
    IEntityChangeListenerService<ApplicationDbContext> entityChangeListenerService,
    ICurrentUserService currentUserService,
    DbContextOptions<ApplicationDbContext> options,
    ILogger<ApplicationDbContext> logger) : 
    DbContext(options),
    ICommonDbContext<ApplicationDbContext>
{
    public IEntityChangeListenerService<ApplicationDbContext> EntityChangeListenerService =>
        entityChangeListenerService;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        this.CommonConfiguring(optionsBuilder);
#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
        
        void LogSensitive(string data)
        {
            logger.LogTrace("DB_SENSITIVE_LOG: {Data}", data);
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
                    p.ParticipantId == currentUserService.TelegramUser.Id));
    }
}