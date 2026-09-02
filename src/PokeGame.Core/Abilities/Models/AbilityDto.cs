using Krakenar.Contracts;

namespace PokeGame.Core.Abilities.Models;

public class AbilityDto : Aggregate
{
  public string Key { get; set; } = string.Empty;

  public string? Name { get; set; }
  public string? Summary { get; set; }
  public string? Content { get; set; }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
