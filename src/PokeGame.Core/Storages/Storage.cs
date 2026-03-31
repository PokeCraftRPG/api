using Logitar.EventSourcing;
using PokeGame.Core.Storages.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Storages;

public class Storage : AggregateRoot
{
  public const string EntityKind = "Storage";

  public new StorageId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;

  private readonly Dictionary<string, long> _storedEntities = [];

  public long AllocatedBytes { get; private set; }
  public long UsedBytes => _storedEntities.Sum(x => x.Value);
  public long RemainingBytes => AllocatedBytes - UsedBytes;

  public Storage() : base()
  {
  }

  public Storage(World world, long allocatedBytes)
    : this(allocatedBytes, world.OwnerId, new StorageId(world.Id))
  {
  }

  public Storage(long allocatedBytes, UserId userId, StorageId storageId)
    : base(storageId.StreamId)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(allocatedBytes, nameof(allocatedBytes));

    Raise(new StorageInitialized(allocatedBytes), userId.ActorId);
  }
  protected virtual void Handle(StorageInitialized @event)
  {
    AllocatedBytes = @event.AllocatedBytes;
  }

  public void Delete(UserId userId)
  {
    if (!IsDeleted)
    {
      Raise(new StorageDeleted(), userId.ActorId);
    }
  }

  public void EnsureAvailable(Entity entity)
  {
    if (!entity.Size.HasValue)
    {
      throw new ArgumentException($"The entity '{entity}' must have a size.", nameof(entity));
    }

    string key = entity.ToString();
    _ = _storedEntities.TryGetValue(key, out long existingBytes);
    if (entity.Size > existingBytes)
    {
      long requiredBytes = entity.Size.Value - existingBytes;
      if (requiredBytes > RemainingBytes)
      {
        throw new NotEnoughStorageException(this, entity, requiredBytes);
      }
    }
  }

  public void Store(Entity entity, UserId userId)
  {
    if (!entity.Size.HasValue)
    {
      throw new ArgumentException($"The entity '{entity}' must have a size.", nameof(entity));
    }

    string key = entity.ToString();
    if (!_storedEntities.TryGetValue(key, out long existingBytes) || existingBytes != entity.Size)
    {
      Raise(new EntityStored(key, entity.Size.Value), userId.ActorId);
    }
  }
  protected virtual void Handle(EntityStored @event)
  {
    _storedEntities[@event.Key] = @event.Size;
  }
}
