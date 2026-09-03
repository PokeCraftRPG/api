using Krakenar.Contracts;

namespace PokeGame.Core.Species.Models;

public class SpeciesDto : Aggregate
{
  public int Number { get; set; }
  public SpeciesCategory Category { get; set; }

  public string Key { get; set; } = string.Empty;

  public string? Name { get; set; }
  public string? Summary { get; set; }
  public string? Content { get; set; }

  public int BaseFriendship { get; set; }
  public int CatchRate { get; set; }
  public GrowthRate GrowthRate { get; set; }
  public SpeciesEggsDto Eggs { get; set; } = new();

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
