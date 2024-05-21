using Common.Database.Traits;
using TelegramBudget.Services.CurrentUser;

namespace TelegramBudget.Data;

public sealed class CreatedByNullableLongChangeListener<TEntity>(ICurrentUserService currentUserService)
    : CreatedByChangeListener<TEntity, long?>
    where TEntity : class, ICreatedByTrait<long?>
{
    protected override long? GetCreatedBy()
    {
        return currentUserService.TelegramUser.Id;
    }
}