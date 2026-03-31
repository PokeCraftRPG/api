using Logitar.EventSourcing;
using PokeGame.Core.Storages;

namespace PokeGame.Infrastructure.Repositories;

internal class StorageRepository : Repository, IStorageRepository
{
  public StorageRepository(IEventStore eventStore) : base(eventStore)
  {
  }

  public async Task<Storage?> LoadAsync(StorageId id, CancellationToken cancellationToken)
  {
    return await LoadAsync<Storage>(id.StreamId, cancellationToken);
  }

  public async Task SaveAsync(Storage storage, CancellationToken cancellationToken)
  {
    await base.SaveAsync(storage, cancellationToken);
  }
}
