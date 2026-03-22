using Krakenar.Contracts;

namespace PokeGame.Core.Moves.Models;

public class MoveModel : Aggregate
{
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }

  public string? Url { get; set; }
  public string? Notes { get; set; }

  public override string ToString() => $"{Name} | {base.ToString()}";
}
