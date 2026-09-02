using Krakenar.Contracts;

namespace PokeGame.Core.Moves.Models;

public class MoveDto : Aggregate
{
  public PokemonType Type { get; set; }
  public MoveCategory Category { get; set; }

  public string Key { get; set; } = string.Empty;

  public string? Name { get; set; }
  public string? Summary { get; set; }
  public string? Content { get; set; }

  public int? Accuracy { get; set; }
  public int? Power { get; set; }
  public int? PowerPoints { get; set; }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
