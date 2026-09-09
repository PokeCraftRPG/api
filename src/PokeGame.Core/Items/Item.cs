using Logitar.EventSourcing;
using PokeGame.Core.Assets;
using PokeGame.Core.Items.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Items;

public sealed class Item : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Item";

  public new ItemId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public ItemCategory Category { get; private set; }

  private Key? _key = null;
  public Key Key => _key ?? throw new InvalidOperationException("The key was not initialized.");

  public Name? Name { get; private set; }
  public Summary? Summary { get; private set; }
  public Content? Content { get; private set; }

  public Price? Price { get; private set; }
  public AssetId? SpriteId { get; private set; }

  public Item() : base()
  {
  }

  public Item(World world, ItemCategory category, Key key, ActorId? actorId = null)
    : this(ItemId.NewId(world.Id), category, key, actorId)
  {
  }

  public Item(ItemId itemId, ItemCategory category, Key key, ActorId? actorId = null)
    : base(itemId.StreamId)
  {
    if (!Enum.IsDefined(category))
    {
      throw new ArgumentOutOfRangeException(nameof(category));
    }

    Raise(new ItemCreated(category, key), actorId);
  }
  private void Handle(ItemCreated @event)
  {
    Category = @event.Category;

    _key = @event.Key;
  }

  public void Delete(ActorId? actorId = null)
  {
    if (!IsDeleted)
    {
      Raise(new ItemDeleted(), actorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);

  public void SetDetails(Name? name, Summary? summary, Content? content, ActorId? actorId = null)
  {
    if (!Equals(Name, name) || !Equals(Summary, summary) || !Equals(Content, content))
    {
      Raise(new ItemDetailsChanged(name, summary, content), actorId);
    }
  }
  private void Handle(ItemDetailsChanged @event)
  {
    Name = @event.Name;
    Summary = @event.Summary;
    Content = @event.Content;
  }

  public void SetKey(Key key, ActorId? actorId = null)
  {
    if (!Equals(Key, key))
    {
      Raise(new ItemKeyChanged(key), actorId);
    }
  }
  private void Handle(ItemKeyChanged @event)
  {
    _key = @event.Key;
  }

  public void SetPrice(Price? price, ActorId? actorId = null)
  {
    if (!Equals(Price, price))
    {
      Raise(new ItemPriceChanged(price), actorId);
    }
  }
  private void Handle(ItemPriceChanged @event)
  {
    Price = @event.Price;
  }

  public void SetSprite(Asset? sprite, ActorId? actorId = null)
  {
    if (sprite is not null)
    {
      WorldMismatchException.ThrowIfMismatch(this, sprite, nameof(sprite));
      InvalidAssetKindException.ThrowIfNotValid(sprite, AssetKind.Image, nameof(SpriteId));
    }

    AssetId? spriteId = sprite?.Id;
    if (!Equals(SpriteId, spriteId))
    {
      Raise(new ItemSpriteChanged(spriteId), actorId);
    }
  }
  private void Handle(ItemSpriteChanged @event)
  {
    SpriteId = @event.SpriteId;
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
