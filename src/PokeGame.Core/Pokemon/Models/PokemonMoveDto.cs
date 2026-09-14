using Krakenar.Contracts.Actors;
using PokeGame.Core.Moves;
using PokeGame.Core.Moves.Models;

namespace PokeGame.Core.Pokemon.Models;

public record PokemonMoveDto
{
  public MoveDto Move { get; set; } = new();

  public int LearnedAtLevel { get; set; }
  public LearningMethod LearningMethod { get; set; }

  public bool IsMastered { get; set; }
  public int PowerPointUpgrades { get; set; }

  public int? Slot { get; set; }

  public Actor CreatedBy { get; set; } = new();
  public DateTime CreatedOn { get; set; }

  public Actor UpdatedBy { get; set; } = new();
  public DateTime UpdatedOn { get; set; }
}
