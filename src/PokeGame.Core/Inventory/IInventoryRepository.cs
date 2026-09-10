namespace PokeGame.Core.Inventory;

public interface IInventoryRepository
{
  Task<TrainerInventory?> LoadAsync(InventoryId id, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<TrainerInventory>> LoadAsync(IEnumerable<InventoryId> ids, CancellationToken cancellationToken = default);

  Task SaveAsync(TrainerInventory inventory, CancellationToken cancellationToken = default);
  Task SaveAsync(IEnumerable<TrainerInventory> inventories, CancellationToken cancellationToken = default);
}
