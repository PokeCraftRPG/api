namespace PokeGame.Core.Species.Events;

public class PokemonSpeciesDeleted : DeleteEvent
{
  public PokemonSpeciesDeleted() : base()
  {
  }

  public PokemonSpeciesDeleted(PokemonSpecies species, Guid userId) : base(species, userId)
  {
  }
}
