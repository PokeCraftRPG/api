using PokeGame.Core.Trainers;

namespace PokeGame.Core.Inventory;

public interface IInventoryRepository
{
  Task<InventoryAggregate> LoadAsync(Trainer trainer, CancellationToken cancellationToken = default);
  Task<InventoryAggregate?> LoadAsync(InventoryId id, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<InventoryAggregate>> LoadAsync(IEnumerable<InventoryId> ids, CancellationToken cancellationToken = default);

  Task SaveAsync(InventoryAggregate inventory, CancellationToken cancellationToken = default);
  Task SaveAsync(IEnumerable<InventoryAggregate> inventory, CancellationToken cancellationToken = default);
}
