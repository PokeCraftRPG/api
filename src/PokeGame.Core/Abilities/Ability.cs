using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Abilities;

public class Ability : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Ability";

  public new AbilityId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public Ability() : base()
  {
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);
}
