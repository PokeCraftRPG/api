using PokeGame.Api.Models.Search;
using PokeGame.Core.Abilities.Models;

namespace PokeGame.Api.Models.Ability;

public record SearchAbilitiesParameters : SearchParameters
{
  public virtual SearchAbilitiesPayload ToPayload()
  {
    SearchAbilitiesPayload payload = new();
    Fill(payload);
    return payload;
  }
}
