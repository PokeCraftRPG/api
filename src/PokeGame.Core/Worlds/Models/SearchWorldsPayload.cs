using Krakenar.Contracts.Search;

namespace PokeGame.Core.Worlds.Models;

public record SearchWorldsPayload : SearchPayload
{
  public new List<WorldSortOption> Sort { get; set; } = [];
}
