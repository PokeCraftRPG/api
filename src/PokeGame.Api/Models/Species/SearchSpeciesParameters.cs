using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Models.Search;
using PokeGame.Core.Species;
using PokeGame.Core.Species.Models;

namespace PokeGame.Api.Models.Species;

public record SearchSpeciesParameters : SearchParameters
{
  [FromQuery(Name = "category")]
  public SpeciesCategory? Category { get; set; }

  [FromQuery(Name = "growth")]
  public GrowthRate? GrowthRate { get; set; }

  [FromQuery(Name = "egg")]
  public EggGroup? EggGroup { get; set; }

  public virtual SearchSpeciesPayload ToPayload()
  {
    SearchSpeciesPayload payload = new();
    payload.Category = Category;
    payload.GrowthRate = GrowthRate;
    payload.EggGroup = EggGroup;
    Fill(payload);
    return payload;
  }
}
