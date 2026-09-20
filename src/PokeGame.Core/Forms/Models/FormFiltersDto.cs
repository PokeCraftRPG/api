using PokeGame.Core.Abilities.Models;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Forms.Models;

public record FormFiltersDto
{
  public List<VarietySummary> Varieties { get; set; } = [];
  public List<AbilitySummary> Abilities { get; set; } = [];
}
