using Krakenar.Contracts;

namespace PokeGame.Core.Regions.Models;

public class RegionModel : Aggregate
{
  public string Key { get; set; } = string.Empty;
  public string? Name { get; set; }
  public string? Description { get; set; }

  public string? Url { get; set; }
  public string? Notes { get; set; }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
