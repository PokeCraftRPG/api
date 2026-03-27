using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Trainers;
using PokeGame.Core.Trainers.Models;
using PokeGame.Extensions;

namespace PokeGame.Controllers;

[ApiController]
[Authorize]
[Route("trainers")]
public class TrainerController : ControllerBase
{
  private readonly ITrainerService _trainerService;

  public TrainerController(ITrainerService trainerService)
  {
    _trainerService = trainerService;
  }

  [HttpPost]
  public async Task<ActionResult<TrainerModel>> CreateAsync([FromBody] CreateOrReplaceTrainerPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceTrainerResult result = await _trainerService.CreateOrReplaceAsync(payload, id: null, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<TrainerModel>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    TrainerModel? trainer = await _trainerService.ReadAsync(id, key: null, cancellationToken);
    return trainer is null ? NotFound() : Ok(trainer);
  }

  [HttpGet("key:{key}")]
  public async Task<ActionResult<TrainerModel>> ReadAsync(string key, CancellationToken cancellationToken)
  {
    TrainerModel? trainer = await _trainerService.ReadAsync(id: null, key, cancellationToken);
    return trainer is null ? NotFound() : Ok(trainer);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<TrainerModel>> ReplaceAsync(Guid id, [FromBody] CreateOrReplaceTrainerPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceTrainerResult result = await _trainerService.CreateOrReplaceAsync(payload, id, cancellationToken);
    return ToActionResult(result);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<TrainerModel>> UpdateAsync(Guid id, [FromBody] UpdateTrainerPayload payload, CancellationToken cancellationToken)
  {
    TrainerModel? trainer = await _trainerService.UpdateAsync(id, payload, cancellationToken);
    return trainer is null ? NotFound() : Ok(trainer);
  }

  private ActionResult<TrainerModel> ToActionResult(CreateOrReplaceTrainerResult result)
  {
    TrainerModel trainer = result.Trainer;
    if (result.Created)
    {
      Uri location = new($"{HttpContext.GetBaseUri()}/trainers/{trainer.Id}", UriKind.Absolute);
      return Created(location, trainer);
    }
    return Ok(trainer);
  }
}
