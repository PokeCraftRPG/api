using Logitar.EventSourcing;
using PokeGame.Core.Inventory;

namespace PokeGame.Infrastructure.Repositories;

internal class InventoryRepository : Repository, IInventoryRepository
{
  public InventoryRepository(IEventStore eventStore) : base(eventStore)
  {
  }

  public async Task<TrainerInventory?> LoadAsync(InventoryId id, CancellationToken cancellationToken)
  {
    return await base.LoadAsync<TrainerInventory>(id.StreamId, cancellationToken);
  }
  public async Task<IReadOnlyCollection<TrainerInventory>> LoadAsync(IEnumerable<InventoryId> ids, CancellationToken cancellationToken)
  {
    return await base.LoadAsync<TrainerInventory>(ids.Select(id => id.StreamId), cancellationToken);
  }

  public async Task SaveAsync(TrainerInventory inventory, CancellationToken cancellationToken)
  {
    await base.SaveAsync(inventory, cancellationToken);
  }
  public async Task SaveAsync(IEnumerable<TrainerInventory> inventories, CancellationToken cancellationToken)
  {
    await base.SaveAsync(inventories, cancellationToken);
  }
}
