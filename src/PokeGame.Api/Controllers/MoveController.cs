using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Extensions;
using PokeGame.Api.Filters;
using PokeGame.Api.Models.Move;
using PokeGame.Core.Moves;
using PokeGame.Core.Moves.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
[Authorize]
[RequireWorld]
[Route("moves")]
public class MoveController : ControllerBase
{
  private readonly IMoveService _moveService;

  public MoveController(IMoveService moveService)
  {
    _moveService = moveService;
  }

  [HttpPost]
  public async Task<ActionResult<MoveDto>> CreateAsync([FromBody] CreateOrReplaceMovePayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceMoveResult result = await _moveService.CreateOrReplaceAsync(payload, id: null, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<MoveDto>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    MoveDto? move = await _moveService.ReadAsync(id, key: null, cancellationToken);
    return move is null ? NotFound() : Ok(move);
  }

  [HttpGet("key:{key}")]
  public async Task<ActionResult<MoveDto>> ReadAsync(string key, CancellationToken cancellationToken)
  {
    MoveDto? move = await _moveService.ReadAsync(id: null, key, cancellationToken);
    return move is null ? NotFound() : Ok(move);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<MoveDto>> ReplaceAsync(Guid id, [FromBody] CreateOrReplaceMovePayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceMoveResult result = await _moveService.CreateOrReplaceAsync(payload, id, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet]
  public async Task<ActionResult<SearchResults<MoveDto>>> SearchAsync([FromQuery] SearchMovesParameters parameters, CancellationToken cancellationToken)
  {
    SearchMovesPayload payload = parameters.ToPayload();
    SearchResults<MoveDto> moves = await _moveService.SearchAsync(payload, cancellationToken);
    return Ok(moves);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<MoveDto>> UpdateAsync(Guid id, [FromBody] UpdateMovePayload payload, CancellationToken cancellationToken)
  {
    MoveDto? move = await _moveService.UpdateAsync(id, payload, cancellationToken);
    return move is null ? NotFound() : Ok(move);
  }

  private ActionResult<MoveDto> ToActionResult(CreateOrReplaceMoveResult result)
  {
    MoveDto move = result.Move;
    if (result.Created)
    {
      Uri location = new($"{HttpContext.GetBaseUrl()}/moves/{move.Id}", UriKind.Absolute);
      return Created(location, move);
    }
    return Ok(move);
  }
}
