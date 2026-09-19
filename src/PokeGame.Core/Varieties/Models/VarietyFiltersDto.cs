using PokeGame.Core.Moves.Models;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Varieties.Models;

public record VarietyFiltersDto
{
  public List<SpeciesSummary> Species { get; set; } = [];
  public List<MoveSummary> Moves { get; set; } = [];
}
