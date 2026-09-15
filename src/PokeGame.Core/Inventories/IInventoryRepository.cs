namespace PokeGame.Core.Inventories;

public interface IInventoryRepository
{
  Task<Inventory?> LoadAsync(InventoryId id, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<Inventory>> LoadAsync(IEnumerable<InventoryId> ids, CancellationToken cancellationToken = default);

  Task SaveAsync(Inventory inventory, CancellationToken cancellationToken = default);
  Task SaveAsync(IEnumerable<Inventory> inventories, CancellationToken cancellationToken = default);
}
