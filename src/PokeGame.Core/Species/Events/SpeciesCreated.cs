using PokeGame.Core.Regions;

namespace PokeGame.Core.Species.Events;

public class SpeciesCreated : CreateEvent
{
  public int Number { get; set; }
  public PokemonCategory Category { get; set; }

  public string Key { get; set; } = string.Empty;
  public string? Name { get; set; }
  public string? Description { get; set; }

  public int BaseFriendship { get; set; }
  public int CatchRate { get; set; }
  public GrowthRate GrowthRate { get; set; }

  public int EggCycles { get; set; }
  public EggGroup PrimaryEggGroup { get; set; }
  public EggGroup? SecondaryEggGroup { get; set; }

  public List<RegionalNumberChange> RegionalNumbers { get; set; } = [];

  public SpeciesCreated() : base()
  {
  }

  public SpeciesCreated(PokemonSpecies species) : base(species)
  {
    Number = species.Number;
    Category = species.Category;

    Key = species.Key;
    Name = species.Name;
    Description = species.Description;

    BaseFriendship = species.BaseFriendship;
    CatchRate = species.CatchRate;
    GrowthRate = species.GrowthRate;

    EggCycles = species.EggCycles;
    PrimaryEggGroup = species.PrimaryEggGroup;
    SecondaryEggGroup = species.SecondaryEggGroup;

    foreach (RegionalNumber regionalNumber in species.RegionalNumbers)
    {
      Region region = regionalNumber.Region ?? throw new ArgumentException("The region is required.", nameof(species));
      RegionalNumbers.Add(new RegionalNumberChange(region.Id, OldNumber: null, regionalNumber.Number));
    }
  }
}
