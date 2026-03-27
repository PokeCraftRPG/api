using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Species;
using PokeGame.Core.Species.Models;
using PokeGame.Extensions;

namespace PokeGame.Controllers;

[ApiController]
[Authorize]
[Route("species")]
public class SpeciesController : ControllerBase
{
  private readonly ISpeciesService _speciesService;

  public SpeciesController(ISpeciesService speciesService)
  {
    _speciesService = speciesService;
  }

  [HttpPost]
  public async Task<ActionResult<SpeciesModel>> CreateAsync([FromBody] CreateOrReplaceSpeciesPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceSpeciesResult result = await _speciesService.CreateOrReplaceAsync(payload, id: null, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<SpeciesModel>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    SpeciesModel? species = await _speciesService.ReadAsync(id, number: null, key: null, cancellationToken);
    return species is null ? NotFound() : Ok(species);
  }

  [HttpGet("key:{key}")]
  public async Task<ActionResult<SpeciesModel>> ReadAsync(string key, CancellationToken cancellationToken)
  {
    SpeciesModel? species = await _speciesService.ReadAsync(id: null, number: null, key, cancellationToken);
    return species is null ? NotFound() : Ok(species);
  }

  [HttpGet("number:{number}")]
  public async Task<ActionResult<SpeciesModel>> ReadAsync(int number, CancellationToken cancellationToken)
  {
    SpeciesModel? species = await _speciesService.ReadAsync(id: null, number, key: null, cancellationToken);
    return species is null ? NotFound() : Ok(species);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<SpeciesModel>> ReplaceAsync(Guid id, [FromBody] CreateOrReplaceSpeciesPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceSpeciesResult result = await _speciesService.CreateOrReplaceAsync(payload, id, cancellationToken);
    return ToActionResult(result);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<SpeciesModel>> UpdateAsync(Guid id, [FromBody] UpdateSpeciesPayload payload, CancellationToken cancellationToken)
  {
    SpeciesModel? species = await _speciesService.UpdateAsync(id, payload, cancellationToken);
    return species is null ? NotFound() : Ok(species);
  }

  private ActionResult<SpeciesModel> ToActionResult(CreateOrReplaceSpeciesResult result)
  {
    SpeciesModel species = result.Species;
    if (result.Created)
    {
      Uri location = new($"{HttpContext.GetBaseUri()}/species/{species.Id}", UriKind.Absolute);
      return Created(location, species);
    }
    return Ok(species);
  }
}
