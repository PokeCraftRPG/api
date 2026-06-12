using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon;

public class Specimen : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Pokemon";

  public new PokemonId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public Specimen() : base()
  {
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);
}
