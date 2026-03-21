using Krakenar.Contracts;

namespace PokeGame.Core.Worlds.Models;

public class WorldModel : Aggregate
{
  public string Slug { get; set; } = string.Empty;
  public string? Name { get; set; }
  public string? Description { get; set; }

  public override string ToString() => $"{Name ?? Slug} | {base.ToString()}";
}
