namespace PokeGame.Core.Species.Events;

public class SpeciesDeleted : DeleteEvent
{
  public SpeciesDeleted() : base()
  {
  }

  public SpeciesDeleted(PokemonSpecies species, Guid userId) : base(species, userId)
  {
  }
}
