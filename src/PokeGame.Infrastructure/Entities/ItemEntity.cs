using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core.Items;
using PokeGame.Core.Items.Events;

namespace PokeGame.Infrastructure.Entities;

internal class ItemEntity : AggregateEntity
{
  public int ItemId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public ItemCategory Category { get; private set; }

  public string Key { get; private set; } = string.Empty;

  public string? Name { get; private set; }
  public string? Summary { get; private set; }
  public string? Content { get; private set; }

  public int? Price { get; private set; }
  public int? Weight { get; private set; }

  public AssetEntity? Sprite { get; private set; }
  public int? SpriteId { get; private set; }

  public ItemEntity(int worldId, ItemCreated @event) : base(@event)
  {
    WorldId = worldId;
    Id = new ItemId(@event.StreamId).EntityId;

    Category = @event.Category;

    Key = @event.Key.Value;
  }

  private ItemEntity() : base()
  {
  }

  public override IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = new(base.GetActorIds());
    if (Sprite is not null)
    {
      actorIds.AddRange(Sprite.GetActorIds());
    }
    return actorIds;
  }

  public void SetCharacteristics(ItemCharacteristicsChanged @event)
  {
    Update(@event);

    Price = @event.Price?.Value;
    Weight = @event.Weight?.Value;
  }

  public void SetDetails(ItemDetailsChanged @event)
  {
    Update(@event);

    Name = @event.Name?.Value;
    Summary = @event.Summary?.Value;
    Content = @event.Content?.Value;
  }

  public void SetKey(ItemKeyChanged @event)
  {
    Update(@event);

    Key = @event.Key.Value;
  }

  public void SetSprite(int? spriteId, ItemSpriteChanged @event)
  {
    Update(@event);

    SpriteId = spriteId;
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
