using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Evolutions;
using PokeGame.Core.Evolutions.Models;
using PokeGame.Extensions;

namespace PokeGame.Controllers;

[ApiController]
[Authorize]
[Route("evolutions")]
public class EvolutionController : ControllerBase
{
  private readonly IEvolutionService _evolutionService;

  public EvolutionController(IEvolutionService evolutionService)
  {
    _evolutionService = evolutionService;
  }

  [HttpPost]
  public async Task<ActionResult<EvolutionModel>> CreateAsync([FromBody] CreateOrReplaceEvolutionPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceEvolutionResult result = await _evolutionService.CreateOrReplaceAsync(payload, id: null, cancellationToken);
    return ToActionResult(result);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<EvolutionModel>> ReplaceAsync(Guid id, [FromBody] CreateOrReplaceEvolutionPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceEvolutionResult result = await _evolutionService.CreateOrReplaceAsync(payload, id, cancellationToken);
    return ToActionResult(result);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<EvolutionModel>> UpdateAsync(Guid id, [FromBody] UpdateEvolutionPayload payload, CancellationToken cancellationToken)
  {
    EvolutionModel? evolution = await _evolutionService.UpdateAsync(id, payload, cancellationToken);
    return evolution is null ? NotFound() : Ok(evolution);
  }

  private ActionResult<EvolutionModel> ToActionResult(CreateOrReplaceEvolutionResult result)
  {
    EvolutionModel evolution = result.Evolution;
    if (result.Created)
    {
      Uri location = new($"{HttpContext.GetBaseUri()}/evolutions/{evolution.Id}", UriKind.Absolute);
      return Created(location, evolution);
    }
    return Ok(evolution);
  }
}
