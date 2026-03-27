using Krakenar.Contracts;

namespace PokeGame.Core.Worlds.Models;

public class WorldModel : Aggregate
{
  public string Key { get; set; } = string.Empty;
  public string? Name { get; set; }
  public string? Description { get; set; }

  // TODO(fpion): Owner

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
