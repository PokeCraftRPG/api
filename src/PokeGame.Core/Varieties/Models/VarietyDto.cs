using Krakenar.Contracts;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Varieties.Models;

public class VarietyDto : Aggregate
{
  public SpeciesDto Species { get; set; } = new();
  public bool IsDefault { get; set; }

  public string Key { get; set; } = string.Empty;

  public string? Name { get; set; }
  public string? Summary { get; set; }
  public string? Content { get; set; }

  public bool CanChangeForm { get; set; }
  public int? GenderRatio { get; set; }
  public string? Genus { get; set; }

  public List<VarietyMoveDto> Moves { get; set; } = [];

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
