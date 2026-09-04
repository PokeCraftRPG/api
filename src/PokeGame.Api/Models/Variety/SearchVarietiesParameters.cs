using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Models.Search;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Api.Models.Variety;

public record SearchVarietiesParameters : SearchParameters
{
  [FromQuery(Name = "species")]
  public string? Species { get; set; }

  [FromQuery(Name = "default")]
  public bool? IsDefault { get; set; }

  [FromQuery(Name = "metamorph")]
  public bool? CanChangeForm { get; set; }

  public virtual SearchVarietiesPayload ToPayload()
  {
    SearchVarietiesPayload payload = new();
    payload.Species = Species;
    payload.IsDefault = IsDefault;
    payload.CanChangeForm = CanChangeForm;
    Fill(payload);
    return payload;
  }
}
