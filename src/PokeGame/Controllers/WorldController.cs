using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;
using PokeGame.Extensions;

namespace PokeGame.Controllers;

[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[Route("worlds")]
public class WorldController : ControllerBase
{
  private readonly IWorldService _worldService;

  public WorldController(IWorldService worldService)
  {
    _worldService = worldService;
  }

  [HttpPost]
  [ProducesResponseType<WorldModel>(StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  public async Task<ActionResult<WorldModel>> CreateAsync([FromBody] CreateOrReplaceWorldPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceWorldResult result = await _worldService.CreateOrReplaceAsync(payload, id: null, cancellationToken);
    return ToActionResult(result);
  }

  [HttpPut("{id}")]
  [ProducesResponseType<WorldModel>(StatusCodes.Status200OK)]
  [ProducesResponseType<WorldModel>(StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  public async Task<ActionResult<WorldModel>> ReplaceAsync(Guid id, [FromBody] CreateOrReplaceWorldPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceWorldResult result = await _worldService.CreateOrReplaceAsync(payload, id, cancellationToken);
    return ToActionResult(result);
  }

  private ActionResult<WorldModel> ToActionResult(CreateOrReplaceWorldResult result)
  {
    WorldModel world = result.World;
    if (result.Created)
    {
      Uri location = new($"{HttpContext.GetBaseUri()}/worlds/{world.Id}", UriKind.Absolute);
      return Created(location, world);
    }
    return Ok(world);
  }
}
