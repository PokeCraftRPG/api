using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Varieties;

public class Variety : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Variety";

  public new VarietyId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public Variety() : base()
  {
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);
}
