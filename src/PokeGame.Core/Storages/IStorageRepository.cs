namespace PokeGame.Core.Storages;

public interface IStorageRepository
{
  Task<Storage?> LoadAsync(StorageId id, CancellationToken cancellationToken = default);

  Task SaveAsync(Storage storage, CancellationToken cancellationToken = default);
}
