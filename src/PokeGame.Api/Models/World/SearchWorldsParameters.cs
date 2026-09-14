using PokeGame.Api.Models.Search;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Api.Models.World;

public record SearchWorldsParameters : SearchParameters
{
  public SearchWorldsPayload ToPayload()
  {
    SearchWorldsPayload payload = new();
    Fill(payload);
    return payload;
  }
}
