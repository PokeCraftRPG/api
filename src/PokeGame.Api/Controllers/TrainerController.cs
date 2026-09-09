using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Filters;
using PokeGame.Api.Models.Trainer;
using PokeGame.Core.Trainers;
using PokeGame.Core.Trainers.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
[Authorize]
[RequireWorld]
[Route("trainers")]
public class TrainerController : ControllerBase
{
  private const string GetByIdRoute = "GetTrainer";

  private readonly ITrainerService _trainerService;

  public TrainerController(ITrainerService trainerService)
  {
    _trainerService = trainerService;
  }

  [HttpPost]
  public async Task<ActionResult<TrainerDto>> CreateAsync([FromBody] CreateOrReplaceTrainerPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceTrainerResult result = await _trainerService.CreateOrReplaceAsync(payload, id: null, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet("{id}", Name = GetByIdRoute)]
  public async Task<ActionResult<TrainerDto>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    TrainerDto? trainer = await _trainerService.ReadAsync(id, key: null, license: null, cancellationToken);
    return trainer is null ? NotFound() : Ok(trainer);
  }

  [HttpGet("key:{key}")]
  public async Task<ActionResult<TrainerDto>> ReadAsync(string key, CancellationToken cancellationToken)
  {
    TrainerDto? trainer = await _trainerService.ReadAsync(id: null, key, license: null, cancellationToken);
    return trainer is null ? NotFound() : Ok(trainer);
  }

  [HttpGet("license:{license}")]
  public async Task<ActionResult<TrainerDto>> ReadByLicenseAsync(string license, CancellationToken cancellationToken)
  {
    TrainerDto? trainer = await _trainerService.ReadAsync(id: null, key: null, license, cancellationToken);
    return trainer is null ? NotFound() : Ok(trainer);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<TrainerDto>> ReplaceAsync(Guid id, [FromBody] CreateOrReplaceTrainerPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceTrainerResult result = await _trainerService.CreateOrReplaceAsync(payload, id, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet]
  public async Task<ActionResult<SearchResults<TrainerDto>>> SearchAsync([FromQuery] SearchTrainersParameters parameters, CancellationToken cancellationToken)
  {
    SearchTrainersPayload payload = parameters.ToPayload();
    SearchResults<TrainerDto> trainers = await _trainerService.SearchAsync(payload, cancellationToken);
    return Ok(trainers);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<TrainerDto>> UpdateAsync(Guid id, [FromBody] UpdateTrainerPayload payload, CancellationToken cancellationToken)
  {
    TrainerDto? trainer = await _trainerService.UpdateAsync(id, payload, cancellationToken);
    return trainer is null ? NotFound() : Ok(trainer);
  }

  private ActionResult<TrainerDto> ToActionResult(CreateOrReplaceTrainerResult result) => result.Created
    ? CreatedAtRoute(GetByIdRoute, new { id = result.Trainer.Id }, result.Trainer)
    : Ok(result.Trainer);
}
