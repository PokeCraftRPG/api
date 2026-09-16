namespace PokeGame.Core.Species.Models;

public record SpeciesFiltersDto
{
  public List<FilterOption> Regions { get; set; } = [];
}
