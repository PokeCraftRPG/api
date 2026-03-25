using Krakenar.Contracts;

namespace PokeGame.Core.Species.Models;

public class SpeciesModel : Aggregate
{
  public int Number { get; set; }
  public PokemonCategory Category { get; set; }

  public string Key { get; set; } = string.Empty;
  public string? Name { get; set; }

  public byte BaseFriendship { get; set; }
  public byte CatchRate { get; set; }
  public GrowthRate GrowthRate { get; set; }

  public byte EggCycles { get; set; }
  public EggGroupsModel EggGroups { get; set; } = new();

  public string? Url { get; set; }
  public string? Notes { get; set; }

  public List<RegionalNumberModel> RegionalNumbers { get; set; } = [];

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
