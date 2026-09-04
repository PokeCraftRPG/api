using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Filters;
using PokeGame.Core.Varieties;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
[Authorize]
[RequireWorld]
[Route("/varieties/{varietyId}/moves")]
public class VarietyMoveController : ControllerBase
{
  private readonly IVarietyService _varietyService;

  public VarietyMoveController(IVarietyService varietyService)
  {
    _varietyService = varietyService;
  }

  [HttpPost]
  public async Task<ActionResult<VarietyDto>> AddAsync(Guid varietyId, [FromBody] SetVarietyMovePayload payload, CancellationToken cancellationToken)
  {
    VarietyDto variety = await _varietyService.SetMoveAsync(varietyId, payload, id: null, cancellationToken);
    return Ok(variety);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<VarietyMoveDto>> ReadAsync(Guid varietyId, Guid id, CancellationToken cancellationToken)
  {
    VarietyDto? variety = await _varietyService.ReadAsync(varietyId, key: null, cancellationToken);
    if (variety is null)
    {
      return NotFound();
    }

    VarietyMoveDto? varietyMove = variety.Moves.SingleOrDefault(x => x.Id == id);
    return varietyMove is null ? NotFound() : Ok(varietyMove);
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult<VarietyDto>> RemoveAsync(Guid varietyId, Guid id, CancellationToken cancellationToken)
  {
    VarietyDto? variety = await _varietyService.RemoveMoveAsync(varietyId, id, cancellationToken);
    return variety is null ? NotFound() : Ok(variety);
  }

  [HttpGet]
  public async Task<ActionResult<SearchResults<VarietyMoveDto>>> SearchAsync(Guid varietyId, CancellationToken cancellationToken)
  {
    VarietyDto? variety = await _varietyService.ReadAsync(varietyId, key: null, cancellationToken);
    if (variety is null)
    {
      return NotFound();
    }

    SearchResults<VarietyMoveDto> results = new(variety.Moves.OrderBy(x => x.LearningMethod.ToString()).ThenBy(x => x.Level ?? 0).ThenBy(x => x.Move.Name ?? x.Move.Key));
    return Ok(results);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<VarietyDto>> SetAsync(Guid varietyId, Guid id, [FromBody] SetVarietyMovePayload payload, CancellationToken cancellationToken)
  {
    VarietyDto variety = await _varietyService.SetMoveAsync(varietyId, payload, id, cancellationToken);
    return Ok(variety);
  }
}
