using Common.Database.Traits;
using TelegramBudget.Services;

namespace TelegramBudget.Data;

public class CreatedByNullableLongChangeListener<TEntity> : CreatedByChangeListener<TEntity, long?>
    where TEntity : class, ICreatedByTrait<long?>
{
    private readonly ICurrentUserService _currentUserService;

    public CreatedByNullableLongChangeListener(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    protected override long? GetCreatedBy()
    {
        return _currentUserService.TelegramUser.Id;
    }
}