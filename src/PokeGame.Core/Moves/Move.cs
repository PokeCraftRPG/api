using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Moves;

public class Move : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Move";

  public new MoveId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public Move() : base()
  {
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);
}
