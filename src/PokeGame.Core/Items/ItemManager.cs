using Logitar.EventSourcing;
using PokeGame.Core.Assets;
using PokeGame.Core.Items.Events;

namespace PokeGame.Core.Items;

public interface IItemManager
{
  Task EnsureUnicityAsync(Item item, CancellationToken cancellationToken = default);
  Task SetSpriteAsync(Item item, Guid? spriteId, string propertyName, CancellationToken cancellationToken = default);
}

internal class ItemManager : IItemManager
{
  private readonly IAssetRepository _assetRepository;
  private readonly IContext _context;
  private readonly IItemQuerier _itemQuerier;

  public ItemManager(IAssetRepository assetRepository, IContext context, IItemQuerier itemQuerier)
  {
    _assetRepository = assetRepository;
    _context = context;
    _itemQuerier = itemQuerier;
  }

  public async Task EnsureUnicityAsync(Item item, CancellationToken cancellationToken)
  {
    Key? key = null;
    foreach (IEvent change in item.Changes)
    {
      if (change is ItemCreated created)
      {
        key = created.Key;
      }
      else if (change is ItemKeyChanged changed)
      {
        key = changed.Key;
      }
    }

    if (key is not null)
    {
      ItemId? itemId = await _itemQuerier.GetIdAsync(key, cancellationToken);
      if (itemId.HasValue && !itemId.Value.Equals(item.Id))
      {
        throw new KeyAlreadyUsedException(item, itemId.Value.EntityId, item.Key, nameof(item.Key));
      }
    }
  }

  public async Task SetSpriteAsync(Item item, Guid? entityId, string propertyName, CancellationToken cancellationToken)
  {
    Asset? sprite = null;
    if (entityId.HasValue)
    {
      AssetId spriteId = new(item.WorldId, entityId.Value);
      sprite = await _assetRepository.LoadAsync(spriteId, cancellationToken);
    }
    item.SetSprite(sprite, _context.ActorId);
  }
}
