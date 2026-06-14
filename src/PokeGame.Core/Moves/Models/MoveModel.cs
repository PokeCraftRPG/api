using Krakenar.Contracts;

namespace PokeGame.Core.Moves.Models;

public class MoveModel : Aggregate
{
  public string Key { get; set; } = string.Empty;
  public string? Name { get; set; }
  public string? Description { get; set; }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
