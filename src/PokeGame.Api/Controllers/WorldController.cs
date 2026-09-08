using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Models.World;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Api.Controllers;

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
  public async Task<ActionResult<WorldDto>> CreateAsync([FromBody] CreateOrReplaceWorldPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceWorldResult result = await _worldService.CreateOrReplaceAsync(payload, id: null, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<WorldDto>> ReadByIdAsync(Guid id, CancellationToken cancellationToken)
  {
    WorldDto? world = await _worldService.ReadAsync(id, key: null, cancellationToken);
    return world is null ? NotFound() : Ok(world);
  }

  [HttpGet("key:{key}")]
  public async Task<ActionResult<WorldDto>> ReadByKeyAsync(string key, CancellationToken cancellationToken)
  {
    WorldDto? world = await _worldService.ReadAsync(id: null, key, cancellationToken);
    return world is null ? NotFound() : Ok(world);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<WorldDto>> ReplaceAsync(Guid id, [FromBody] CreateOrReplaceWorldPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceWorldResult result = await _worldService.CreateOrReplaceAsync(payload, id, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet]
  public async Task<ActionResult<SearchResults<WorldDto>>> SearchAsync([FromQuery] SearchWorldsParameters parameters, CancellationToken cancellationToken)
  {
    SearchWorldsPayload payload = parameters.ToPayload();
    SearchResults<WorldDto> worlds = await _worldService.SearchAsync(payload, cancellationToken);
    return Ok(worlds);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<WorldDto>> UpdateAsync(Guid id, [FromBody] UpdateWorldPayload payload, CancellationToken cancellationToken)
  {
    WorldDto? world = await _worldService.UpdateAsync(id, payload, cancellationToken);
    return world is null ? NotFound() : Ok(world);
  }

  private ActionResult<WorldDto> ToActionResult(CreateOrReplaceWorldResult result) => result.Created
    ? CreatedAtAction(nameof(ReadByIdAsync), new { id = result.World.Id }, result.World)
    : Ok(result.World);
}
