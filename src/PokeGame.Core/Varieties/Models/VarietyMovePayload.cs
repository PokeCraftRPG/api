namespace PokeGame.Core.Varieties.Models;

public record VarietyMovePayload
{
  public string Move { get; set; }
  public int Level { get; set; } // TODO(fpion): should be nullable, but not null in CreateOrReplace payload (or treated as 0)

  public VarietyMovePayload() : this(string.Empty, default)
  {
  }

  public VarietyMovePayload(string move, int level)
  {
    Move = move;
    Level = level;
  }
}
