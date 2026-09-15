using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Filters;
using PokeGame.Core.Rosters;
using PokeGame.Core.Rosters.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
[Authorize]
[RequireWorld]
[Route("trainers/{trainerId}/tags")]
public class TagController : ControllerBase
{
  private const string GetByIdRoute = "GetTag";

  private readonly IRosterService _rosterService;

  public TagController(IRosterService rosterService)
  {
    _rosterService = rosterService;
  }

  [HttpPost]
  public async Task<ActionResult<TagDto>> CreateAsync(Guid trainerId, [FromBody] CreateOrReplaceTagPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceTagResult result = await _rosterService.CreateOrReplaceTagAsync(trainerId, payload, tagId: null, cancellationToken);
    return ToActionResult(trainerId, result);
  }

  [HttpDelete("{tagId}")]
  public async Task<ActionResult<TagDto>> DeleteAsync(Guid trainerId, Guid tagId, CancellationToken cancellationToken)
  {
    TagDto? tag = await _rosterService.DeleteTagAsync(trainerId, tagId, cancellationToken);
    return tag is null ? NotFound() : Ok(tag);
  }

  [HttpGet("{tagId}", Name = GetByIdRoute)]
  public async Task<ActionResult<TagDto>> ReadAsync(Guid trainerId, Guid tagId, CancellationToken cancellationToken)
  {
    TagDto? tag = await _rosterService.ReadTagAsync(trainerId, tagId, cancellationToken);
    return tag is null ? NotFound() : Ok(tag);
  }

  [HttpPut("{tagId}")]
  public async Task<ActionResult<TagDto>> ReplaceAsync(Guid trainerId, Guid tagId, [FromBody] CreateOrReplaceTagPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceTagResult result = await _rosterService.CreateOrReplaceTagAsync(trainerId, payload, tagId, cancellationToken);
    return ToActionResult(trainerId, result);
  }

  [HttpPatch("{tagId}")]
  public async Task<ActionResult<TagDto>> UpdateAsync(Guid trainerId, Guid tagId, [FromBody] UpdateTagPayload payload, CancellationToken cancellationToken)
  {
    TagDto? tag = await _rosterService.UpdateTagAsync(trainerId, tagId, payload, cancellationToken);
    return tag is null ? NotFound() : Ok(tag);
  }

  private ActionResult<TagDto> ToActionResult(Guid trainerId, CreateOrReplaceTagResult result) => result.Created
    ? CreatedAtRoute(GetByIdRoute, new { trainerId, tagId = result.Tag.Id }, result.Tag)
    : Ok(result.Tag);
}
