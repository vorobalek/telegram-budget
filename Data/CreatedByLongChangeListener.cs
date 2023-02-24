using Common.Database.Traits;
using TelegramBudget.Services;

namespace TelegramBudget.Data;

public class CreatedByLongChangeListener<TEntity> : CreatedByChangeListener<TEntity, long>
    where TEntity : class, ICreatedByTrait<long>
{
    private readonly ICurrentUserService _currentUserService;

    public CreatedByLongChangeListener(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    protected override long GetCreatedBy()
    {
        return _currentUserService.TelegramUser.Id;
    }
}