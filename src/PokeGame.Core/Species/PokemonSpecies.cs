using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Species;

public class PokemonSpecies : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Species";

  public new SpeciesId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public PokemonSpecies() : base()
  {
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);
}
