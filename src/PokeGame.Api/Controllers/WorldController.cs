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
  private const string GetByIdRoute = "GetWorld";

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

  [HttpGet("{id}", Name = GetByIdRoute)]
  public async Task<ActionResult<WorldDto>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    WorldDto? world = await _worldService.ReadAsync(id, key: null, cancellationToken);
    return world is null ? NotFound() : Ok(world);
  }

  [HttpGet("key:{key}")]
  public async Task<ActionResult<WorldDto>> ReadAsync(string key, CancellationToken cancellationToken)
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
    ? CreatedAtRoute(GetByIdRoute, new { id = result.World.Id }, result.World)
    : Ok(result.World);
}
