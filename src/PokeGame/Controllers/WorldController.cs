using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;
using PokeGame.Extensions;

namespace PokeGame.Controllers;

[ApiController]
[Authorize]
[Route("worlds")]
public class WorldController : ControllerBase
{
  private readonly IWorldService _worldService;

  public WorldController(IWorldService worldService)
  {
    _worldService = worldService;
  }

  [HttpPost]
  public async Task<ActionResult<WorldModel>> CreateAsync([FromBody] CreateOrReplaceWorldPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceWorldResult result = await _worldService.CreateOrReplaceAsync(payload, id: null, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<WorldModel>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    WorldModel? world = await _worldService.ReadAsync(id, slug: null, cancellationToken);
    return world is null ? NotFound() : Ok(world);
  }

  [HttpGet("slug:{slug}")]
  public async Task<ActionResult<WorldModel>> ReadAsync(string slug, CancellationToken cancellationToken)
  {
    WorldModel? world = await _worldService.ReadAsync(id: null, slug, cancellationToken);
    return world is null ? NotFound() : Ok(world);
  }

  [HttpPut("{id}")]
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
