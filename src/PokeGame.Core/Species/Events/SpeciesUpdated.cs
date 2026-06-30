namespace PokeGame.Core.Species.Events;

public class SpeciesUpdated : UpdateEvent
{
  public Change<string>? Key { get; set; }
  public Change<string>? Name { get; set; }
  public Change<string>? Description { get; set; }

  public Change<int>? BaseFriendship { get; set; }
  public Change<int>? CatchRate { get; set; }
  public Change<GrowthRate>? GrowthRate { get; set; }

  public Change<int>? EggCycles { get; set; }
  public Change<EggGroup>? PrimaryEggGroup { get; set; }
  public Change<EggGroup?>? SecondaryEggGroup { get; set; }

  public SpeciesUpdated() : base()
  {
  }

  public SpeciesUpdated(PokemonSpecies species) : base(species)
  {
  }
}
