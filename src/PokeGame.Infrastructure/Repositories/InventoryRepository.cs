using Logitar.EventSourcing;
using PokeGame.Core.Inventory;
using PokeGame.Core.Trainers;

namespace PokeGame.Infrastructure.Repositories;

internal class InventoryRepository : Repository, IInventoryRepository
{
  public InventoryRepository(IEventStore eventStore) : base(eventStore)
  {
  }

  public async Task<InventoryAggregate> LoadAsync(Trainer trainer, CancellationToken cancellationToken)
  {
    return await LoadAsync(new InventoryId(trainer.Id), cancellationToken) ?? new(trainer);
  }
  public async Task<InventoryAggregate?> LoadAsync(InventoryId id, CancellationToken cancellationToken)
  {
    return await LoadAsync<InventoryAggregate>(id.StreamId, cancellationToken);
  }
  public async Task<IReadOnlyCollection<InventoryAggregate>> LoadAsync(IEnumerable<InventoryId> ids, CancellationToken cancellationToken)
  {
    return await LoadAsync<InventoryAggregate>(ids.Select(id => id.StreamId), cancellationToken);
  }

  public async Task SaveAsync(InventoryAggregate inventory, CancellationToken cancellationToken)
  {
    await base.SaveAsync(inventory, cancellationToken);
  }
  public async Task SaveAsync(IEnumerable<InventoryAggregate> inventories, CancellationToken cancellationToken)
  {
    await base.SaveAsync(inventories, cancellationToken);
  }
}
