using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Models.Variety;

public record SearchVarietiesParameters : SearchParameters
{
  [FromQuery(Name = "species")]
  public Guid? SpeciesId { get; set; }

  [FromQuery(Name = "metamorph")]
  public bool? CanChangeForm { get; set; }

  public virtual SearchVarietiesPayload ToPayload()
  {
    SearchVarietiesPayload payload = new()
    {
      SpeciesId = SpeciesId,
      CanChangeForm = CanChangeForm
    };
    Fill(payload);

    foreach (SortOption sort in ((SearchPayload)payload).Sort)
    {
      if (Enum.TryParse(sort.Field, out VarietySort field))
      {
        payload.Sort.Add(new VarietySortOption(field, sort.IsDescending));
      }
    }

    return payload;
  }
}
