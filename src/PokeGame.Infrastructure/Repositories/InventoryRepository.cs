using Logitar.EventSourcing;
using PokeGame.Core.Inventories;

namespace PokeGame.Infrastructure.Repositories;

internal class InventoryRepository : Repository, IInventoryRepository
{
  public InventoryRepository(IEventStore eventStore) : base(eventStore)
  {
  }

  public async Task<Inventory?> LoadAsync(InventoryId id, CancellationToken cancellationToken)
  {
    return await base.LoadAsync<Inventory>(id.StreamId, cancellationToken);
  }
  public async Task<IReadOnlyCollection<Inventory>> LoadAsync(IEnumerable<InventoryId> ids, CancellationToken cancellationToken)
  {
    return await base.LoadAsync<Inventory>(ids.Select(id => id.StreamId), cancellationToken);
  }

  public async Task SaveAsync(Inventory inventory, CancellationToken cancellationToken)
  {
    await base.SaveAsync(inventory, cancellationToken);
  }
  public async Task SaveAsync(IEnumerable<Inventory> inventories, CancellationToken cancellationToken)
  {
    await base.SaveAsync(inventories, cancellationToken);
  }
}
