using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Extensions;
using PokeGame.Api.Filters;
using PokeGame.Api.Models.Species;
using PokeGame.Core.Species;
using PokeGame.Core.Species.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
[Authorize]
[RequireWorld]
[Route("species")]
public class SpeciesController : ControllerBase
{
  private readonly ISpeciesService _speciesService;

  public SpeciesController(ISpeciesService speciesService)
  {
    _speciesService = speciesService;
  }

  [HttpPost]
  public async Task<ActionResult<SpeciesDto>> CreateAsync([FromBody] CreateOrReplaceSpeciesPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceSpeciesResult result = await _speciesService.CreateOrReplaceAsync(payload, id: null, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<SpeciesDto>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    SpeciesDto? species = await _speciesService.ReadAsync(id, number: null, key: null, cancellationToken);
    return species is null ? NotFound() : Ok(species);
  }

  [HttpGet("number:{number}")]
  public async Task<ActionResult<SpeciesDto>> ReadAsync(int number, CancellationToken cancellationToken)
  {
    SpeciesDto? species = await _speciesService.ReadAsync(id: null, number, key: null, cancellationToken);
    return species is null ? NotFound() : Ok(species);
  }

  [HttpGet("key:{key}")]
  public async Task<ActionResult<SpeciesDto>> ReadAsync(string key, CancellationToken cancellationToken)
  {
    SpeciesDto? species = await _speciesService.ReadAsync(id: null, number: null, key, cancellationToken);
    return species is null ? NotFound() : Ok(species);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<SpeciesDto>> ReplaceAsync(Guid id, [FromBody] CreateOrReplaceSpeciesPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceSpeciesResult result = await _speciesService.CreateOrReplaceAsync(payload, id, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet]
  public async Task<ActionResult<SearchResults<SpeciesDto>>> SearchAsync([FromQuery] SearchSpeciesParameters parameters, CancellationToken cancellationToken)
  {
    SearchSpeciesPayload payload = parameters.ToPayload();
    SearchResults<SpeciesDto> species = await _speciesService.SearchAsync(payload, cancellationToken);
    return Ok(species);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<SpeciesDto>> UpdateAsync(Guid id, [FromBody] UpdateSpeciesPayload payload, CancellationToken cancellationToken)
  {
    SpeciesDto? species = await _speciesService.UpdateAsync(id, payload, cancellationToken);
    return species is null ? NotFound() : Ok(species);
  }

  private ActionResult<SpeciesDto> ToActionResult(CreateOrReplaceSpeciesResult result)
  {
    SpeciesDto species = result.Species;
    if (result.Created)
    {
      Uri location = new($"{HttpContext.GetBaseUrl()}/species/{species.Id}", UriKind.Absolute);
      return Created(location, species);
    }
    return Ok(species);
  }
}
