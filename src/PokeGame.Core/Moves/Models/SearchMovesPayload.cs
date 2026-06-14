using Krakenar.Contracts.Search;

namespace PokeGame.Core.Moves.Models;

public record SearchMovesPayload : SearchPayload
{
  public new List<MoveSortOption> Sort { get; set; } = [];
}
