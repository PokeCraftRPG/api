using Logitar.EventSourcing;

namespace PokeGame.Core.Worlds;

public class World : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "World";

  public new WorldId Id => new(base.Id);
  public Guid EntityId => Id.EntityId;

  public World() : base()
  {
  }

  public Entity GetEntity() => new(EntityKind, EntityId);
}
