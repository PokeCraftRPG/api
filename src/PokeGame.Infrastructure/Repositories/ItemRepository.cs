using Logitar.EventSourcing;
using PokeGame.Core.Items;

namespace PokeGame.Infrastructure.Repositories;

internal class ItemRepository : Repository, IItemRepository
{
  public ItemRepository(IEventStore eventStore) : base(eventStore)
  {
  }

  public async Task<Item?> LoadAsync(ItemId id, CancellationToken cancellationToken)
  {
    return await LoadAsync<Item>(id.StreamId, cancellationToken);
  }
  public async Task<IReadOnlyCollection<Item>> LoadAsync(IEnumerable<ItemId> ids, CancellationToken cancellationToken)
  {
    return await LoadAsync<Item>(ids.Select(id => id.StreamId), cancellationToken);
  }

  public async Task SaveAsync(Item item, CancellationToken cancellationToken)
  {
    await base.SaveAsync(item, cancellationToken);
  }
  public async Task SaveAsync(IEnumerable<Item> items, CancellationToken cancellationToken)
  {
    await base.SaveAsync(items, cancellationToken);
  }
}
