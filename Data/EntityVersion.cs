using Common.Database;
using Common.Database.Hosts;
using Common.Database.Traits;

namespace TelegramBudget.Data;

public abstract class EntityVersion<THost, TKey, TModel> :
    Entity<TModel>,
    ICreatedAtTrait,
    ICreatedByTrait<long?>,
    IVersionModelHost<THost, TKey, TModel>
    where THost : Entity<THost>, IVersionHost<THost, TKey, TModel>, IIdTrait<TKey>, new()
    where TKey : IEquatable<TKey>
    where TModel : Entity<TModel>, IVersionModelHost<THost, TKey, TModel>, new()
{
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public int Number { get; set; }
    public TKey EntityId { get; set; } = default!;
    public THost? Serialized { get; set; }
}