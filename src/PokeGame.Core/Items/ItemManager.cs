using PokeGame.Core.Worlds;

namespace PokeGame.Core.Items;

public interface IItemManager
{
  Task<Item> FindAsync(string item, string propertyName, CancellationToken cancellationToken = default);
}

internal class ItemManager : IItemManager
{
  private readonly IContext _context;
  private readonly IItemQuerier _itemQuerier;
  private readonly IItemRepository _itemRepository;

  public ItemManager(IContext context, IItemQuerier itemQuerier, IItemRepository itemRepository)
  {
    _context = context;
    _itemQuerier = itemQuerier;
    _itemRepository = itemRepository;
  }

  public async Task<Item> FindAsync(string idOrKey, string propertyName, CancellationToken cancellationToken)
  {
    WorldId worldId = _context.WorldId;

    if (Guid.TryParse(idOrKey, out Guid id))
    {
      ItemId itemId = new(worldId, id);
      Item? item = await _itemRepository.LoadAsync(itemId, cancellationToken);
      if (item is not null)
      {
        return item;
      }
    }

    ItemId? foundId = await _itemQuerier.FindIdAsync(idOrKey, cancellationToken);
    if (!foundId.HasValue)
    {
      throw new ItemNotFoundException(worldId, idOrKey, propertyName);
    }

    return await _itemRepository.LoadAsync(foundId.Value, cancellationToken)
      ?? throw new InvalidOperationException($"The item 'Id={foundId}' was not loaded.");
  }
}
