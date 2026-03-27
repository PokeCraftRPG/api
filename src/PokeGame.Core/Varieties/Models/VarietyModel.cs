using Krakenar.Contracts;
using PokeGame.Core.Forms.Models;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Varieties.Models;

public class VarietyModel : Aggregate
{
  public SpeciesModel Species { get; set; } = new();
  public bool IsDefault { get; set; }

  public string Key { get; set; } = string.Empty;
  public string? Name { get; set; }
  public string? Genus { get; set; }
  public string? Description { get; set; }

  public int? GenderRatio { get; set; }

  public string? Url { get; set; }
  public string? Notes { get; set; }

  public bool CanChangeForm { get; set; }
  public List<FormModel> Forms { get; set; } = [];

  public List<VarietyMoveModel> Moves { get; set; } = [];

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
