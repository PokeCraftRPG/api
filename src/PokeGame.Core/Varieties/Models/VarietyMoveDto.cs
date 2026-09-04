using Krakenar.Contracts.Actors;
using PokeGame.Core.Moves;
using PokeGame.Core.Moves.Models;

namespace PokeGame.Core.Varieties.Models;

public class VarietyMoveDto
{
  public Guid Id { get; set; }

  public MoveDto Move { get; set; } = new();

  public LearningMethod LearningMethod { get; set; }
  public int? Level { get; set; }

  public Actor CreatedBy { get; set; } = new();
  public DateTime CreatedOn { get; set; }

  public Actor UpdatedBy { get; set; } = new();
  public DateTime UpdatedOn { get; set; }

  public override bool Equals(object? obj) => obj is VarietyMoveDto varietyMove && varietyMove.Id == Id;
  public override int GetHashCode() => Id.GetHashCode();
  public override string ToString() => $"{base.ToString()} (Id={Id})";
}
