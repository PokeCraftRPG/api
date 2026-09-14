using PokeGame.Api.Models.Search;
using PokeGame.Core.Regions.Models;

namespace PokeGame.Api.Models.Region;

public record SearchRegionsParameters : SearchParameters
{
  public SearchRegionsPayload ToPayload()
  {
    SearchRegionsPayload payload = new();
    Fill(payload);
    return payload;
  }
}
