using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Evolutions;

public class Evolution : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Evolution";

  public new EvolutionId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public Evolution() : base()
  {
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);
}
