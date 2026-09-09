using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Filters;
using PokeGame.Api.Models.Evolution;
using PokeGame.Core.Evolutions;
using PokeGame.Core.Evolutions.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
[Authorize]
[RequireWorld]
[Route("evolutions")]
public class EvolutionController : ControllerBase
{
  private const string GetByIdRoute = "GetEvolution";

  private readonly IEvolutionService _evolutionService;

  public EvolutionController(IEvolutionService evolutionService)
  {
    _evolutionService = evolutionService;
  }

  [HttpPost]
  public async Task<ActionResult<EvolutionDto>> CreateAsync([FromBody] CreateOrReplaceEvolutionPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceEvolutionResult result = await _evolutionService.CreateOrReplaceAsync(payload, id: null, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet("{id}", Name = GetByIdRoute)]
  public async Task<ActionResult<EvolutionDto>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    EvolutionDto? evolution = await _evolutionService.ReadAsync(id, cancellationToken);
    return evolution is null ? NotFound() : Ok(evolution);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<EvolutionDto>> ReplaceAsync(Guid id, [FromBody] CreateOrReplaceEvolutionPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceEvolutionResult result = await _evolutionService.CreateOrReplaceAsync(payload, id, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet]
  public async Task<ActionResult<SearchResults<EvolutionDto>>> SearchAsync([FromQuery] SearchEvolutionsParameters parameters, CancellationToken cancellationToken)
  {
    SearchEvolutionsPayload payload = parameters.ToPayload();
    SearchResults<EvolutionDto> evolutions = await _evolutionService.SearchAsync(payload, cancellationToken);
    return Ok(evolutions);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<EvolutionDto>> UpdateAsync(Guid id, [FromBody] UpdateEvolutionPayload payload, CancellationToken cancellationToken)
  {
    EvolutionDto? evolution = await _evolutionService.UpdateAsync(id, payload, cancellationToken);
    return evolution is null ? NotFound() : Ok(evolution);
  }

  private ActionResult<EvolutionDto> ToActionResult(CreateOrReplaceEvolutionResult result) => result.Created
    ? CreatedAtRoute(GetByIdRoute, new { id = result.Evolution.Id }, result.Evolution)
    : Ok(result.Evolution);
}
