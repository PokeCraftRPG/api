using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Models.Search;
using PokeGame.Core.Evolutions;
using PokeGame.Core.Evolutions.Models;

namespace PokeGame.Api.Models.Evolution;

public record SearchEvolutionsParameters : SearchParameters
{
  [FromQuery(Name = "source")]
  public string? Source { get; set; }

  [FromQuery(Name = "target")]
  public string? Target { get; set; }

  [FromQuery(Name = "trigger")]
  public EvolutionTrigger? Trigger { get; set; }

  public virtual SearchEvolutionsPayload ToPayload()
  {
    SearchEvolutionsPayload payload = new();
    payload.Source = Source;
    payload.Target = Target;
    payload.Trigger = Trigger;
    Fill(payload);
    return payload;
  }
}
