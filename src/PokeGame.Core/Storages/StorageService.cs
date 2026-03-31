using PokeGame.Core.Worlds;

namespace PokeGame.Core.Storages;

public interface IStorageService
{
  Task ExecuteWithQuotaAsync(IEntityProvider resource, Func<Task>? function = null, CancellationToken cancellationToken = default);
}

internal class StorageService : IStorageService
{
  private readonly IContext _context;
  private readonly StorageSettings _settings;
  private readonly IStorageRepository _storageRepository;

  public StorageService(IContext context, StorageSettings settings, IStorageRepository storageRepository)
  {
    _context = context;
    _settings = settings;
    _storageRepository = storageRepository;
  }

  public async Task ExecuteWithQuotaAsync(IEntityProvider resource, Func<Task>? function, CancellationToken cancellationToken)
  {
    Entity entity = resource.GetEntity();
    WorldId worldId = GetWorldId(entity);
    StorageId storageId = new(worldId);
    UserId userId = _context.UserId;
    Storage storage = await _storageRepository.LoadAsync(storageId, cancellationToken) ?? new(_settings.AllocatedBytes, userId, storageId);

    storage.EnsureAvailable(entity);

    if (function is not null)
    {
      await function();
    }

    storage.Store(entity, userId);

    await _storageRepository.SaveAsync(storage, cancellationToken);
  }

  private static WorldId GetWorldId(Entity entity)
  {
    if (entity.WorldId.HasValue)
    {
      return entity.WorldId.Value;
    }
    else if (entity.Kind == World.EntityKind)
    {
      return new WorldId(entity.Id);
    }
    throw new ArgumentException($"No world ID was resolved from entity '{entity}'.", nameof(entity));
  }
}
