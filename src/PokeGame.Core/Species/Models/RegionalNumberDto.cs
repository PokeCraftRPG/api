using Krakenar.Contracts.Actors;
using PokeGame.Core.Regions.Models;

namespace PokeGame.Core.Species.Models;

public record RegionalNumberDto
{
  public RegionDto Region { get; set; } = new();
  public int Number { get; set; }

  public Actor CreatedBy { get; set; } = new();
  public DateTime CreatedOn { get; set; }

  public Actor UpdatedBy { get; set; } = new();
  public DateTime UpdatedOn { get; set; }
}
