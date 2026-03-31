using PokeGame.Core;
using PokeGame.Core.Storages.Events;

namespace PokeGame.Infrastructure.Entities;

internal class StorageDetailEntity
{
  public int StorageDetailId { get; private set; }
  public string Key { get; private set; } = string.Empty;

  public StorageSummaryEntity? Summary { get; private set; }
  public int WorldId { get; private set; }

  public string EntityKind { get; private set; } = string.Empty;
  public Guid EntityId { get; private set; }

  public long Size { get; private set; }

  public StorageDetailEntity(StorageSummaryEntity summary, EntityStored @event)
  {
    Key = @event.Key;

    Summary = summary;
    WorldId = summary.WorldId;

    Entity entity = Entity.Parse(@event.Key);
    EntityKind = entity.Kind;
    EntityId = entity.Id;

    Size = @event.Size;
  }

  private StorageDetailEntity()
  {
  }

  public void Update(EntityStored @event)
  {
    Size = @event.Size;
  }

  public override bool Equals(object? obj) => obj is StorageDetailEntity detail && detail.StorageDetailId == StorageDetailId;
  public override int GetHashCode() => StorageDetailId.GetHashCode();
  public override string ToString() => $"{Key} (StorageDetailId={StorageDetailId})";
}
