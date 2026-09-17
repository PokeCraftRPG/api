using PokeGame.Core.Regions.Models;

namespace PokeGame.Core.Species.Models;

public record SpeciesFiltersDto
{
  public List<RegionSummary> Regions { get; set; } = [];
}
