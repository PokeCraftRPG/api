using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Items;

public class Item : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Item";

  public new ItemId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public Item() : base()
  {
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);
}
