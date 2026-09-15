using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Filters;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Rosters;
using PokeGame.Core.Rosters.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
[Authorize]
[RequireWorld]
public class RosterController : ControllerBase
{
  private readonly IRosterService _rosterService;

  public RosterController(IRosterService rosterService)
  {
    _rosterService = rosterService;
  }

  [HttpPut("/pokemon/{id}/roster")]
  public async Task<ActionResult<PokemonDto>> SetEntryAsync(Guid id, [FromBody] SetRosterEntryPayload payload, CancellationToken cancellationToken)
  {
    PokemonDto? pokemon = await _rosterService.SetEntryAsync(id, payload, cancellationToken);
    return pokemon is null ? NotFound() : Ok(pokemon);
  }
}
