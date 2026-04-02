using Krakenar.Contracts.Search;

namespace PokeGame.Core.Varieties.Models;

public record SearchVarietiesPayload : SearchPayload
{
  public Guid? SpeciesId { get; set; }
  public bool? CanChangeForm { get; set; }

  public new List<VarietySortOption> Sort { get; set; } = [];
}
