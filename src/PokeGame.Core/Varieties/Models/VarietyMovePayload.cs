namespace PokeGame.Core.Varieties.Models;

public record VarietyMovePayload
{
  public string Move { get; set; }
  public int Level { get; set; }

  public VarietyMovePayload() : this(string.Empty, default)
  {
  }

  public VarietyMovePayload(string move, int level)
  {
    Move = move;
    Level = level;
  }
}
