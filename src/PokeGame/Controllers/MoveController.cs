using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Moves;
using PokeGame.Core.Moves.Models;
using PokeGame.Extensions;
using PokeGame.Models.Move;

namespace PokeGame.Controllers;

[ApiController]
[Authorize]
[Route("moves")]
public class MoveController : ControllerBase
{
  private readonly IMoveService _moveService;

  public MoveController(IMoveService moveService)
  {
    _moveService = moveService;
  }

  [HttpPost]
  public async Task<ActionResult<MoveModel>> CreateAsync([FromBody] CreateOrReplaceMovePayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceMoveResult result = await _moveService.CreateOrReplaceAsync(payload, id: null, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<MoveModel>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    MoveModel? move = await _moveService.ReadAsync(id, key: null, cancellationToken);
    return move is null ? NotFound() : Ok(move);
  }

  [HttpGet("key:{key}")]
  public async Task<ActionResult<MoveModel>> ReadAsync(string key, CancellationToken cancellationToken)
  {
    MoveModel? move = await _moveService.ReadAsync(id: null, key, cancellationToken);
    return move is null ? NotFound() : Ok(move);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<MoveModel>> ReplaceAsync(Guid id, [FromBody] CreateOrReplaceMovePayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceMoveResult result = await _moveService.CreateOrReplaceAsync(payload, id, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet]
  public async Task<ActionResult<SearchResults<MoveModel>>> SearchAsync([FromQuery] SearchMovesParameters parameters, CancellationToken cancellationToken)
  {
    SearchMovesPayload payload = parameters.ToPayload();
    SearchResults<MoveModel> moves = await _moveService.SearchAsync(payload, cancellationToken);
    return Ok(moves);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<MoveModel>> UpdateAsync(Guid id, [FromBody] UpdateMovePayload payload, CancellationToken cancellationToken)
  {
    MoveModel? move = await _moveService.UpdateAsync(id, payload, cancellationToken);
    return move is null ? NotFound() : Ok(move);
  }

  private ActionResult<MoveModel> ToActionResult(CreateOrReplaceMoveResult result)
  {
    MoveModel move = result.Move;
    if (result.Created)
    {
      Uri location = new($"{HttpContext.GetBaseUri()}/moves/{move.Id}", UriKind.Absolute);
      return Created(location, move);
    }
    return Ok(move);
  }
}
