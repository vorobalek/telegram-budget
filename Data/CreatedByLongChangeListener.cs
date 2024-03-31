using Common.Database.Traits;
using TelegramBudget.Services;

namespace TelegramBudget.Data;

public class CreatedByLongChangeListener<TEntity>(ICurrentUserService currentUserService)
    : CreatedByChangeListener<TEntity, long>
    where TEntity : class, ICreatedByTrait<long>
{
    protected override long GetCreatedBy()
    {
        return currentUserService.TelegramUser.Id;
    }
}