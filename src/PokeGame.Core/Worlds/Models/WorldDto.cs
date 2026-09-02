using Krakenar.Contracts;
using Krakenar.Contracts.Actors;

namespace PokeGame.Core.Worlds.Models;

public class WorldDto : Aggregate
{
  public Actor Owner { get; set; } = new();

  public string Key { get; set; } = string.Empty;

  public string? Name { get; set; }
  public string? Summary { get; set; }
  public string? Content { get; set; }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
