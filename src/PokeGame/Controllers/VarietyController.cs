using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Varieties;
using PokeGame.Core.Varieties.Models;
using PokeGame.Extensions;
using PokeGame.Models.Variety;

namespace PokeGame.Controllers;

[ApiController]
[Authorize]
[Route("varieties")]
public class VarietyController : ControllerBase
{
  private readonly IVarietyService _varietyService;

  public VarietyController(IVarietyService varietyService)
  {
    _varietyService = varietyService;
  }

  [HttpPost]
  public async Task<ActionResult<VarietyModel>> CreateAsync([FromBody] CreateOrReplaceVarietyPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceVarietyResult result = await _varietyService.CreateOrReplaceAsync(payload, id: null, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<VarietyModel>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    VarietyModel? variety = await _varietyService.ReadAsync(id, key: null, cancellationToken);
    return variety is null ? NotFound() : Ok(variety);
  }

  [HttpGet("key:{key}")]
  public async Task<ActionResult<VarietyModel>> ReadAsync(string key, CancellationToken cancellationToken)
  {
    VarietyModel? variety = await _varietyService.ReadAsync(id: null, key, cancellationToken);
    return variety is null ? NotFound() : Ok(variety);
  }

  [HttpGet]
  public async Task<ActionResult<SearchResults<VarietyModel>>> SearchAsync([FromQuery] SearchVarietiesParameters parameters, CancellationToken cancellationToken)
  {
    SearchVarietiesPayload payload = parameters.ToPayload();
    SearchResults<VarietyModel> varieties = await _varietyService.SearchAsync(payload, cancellationToken);
    return Ok(varieties);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<VarietyModel>> ReplaceAsync(Guid id, [FromBody] CreateOrReplaceVarietyPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceVarietyResult result = await _varietyService.CreateOrReplaceAsync(payload, id, cancellationToken);
    return ToActionResult(result);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<VarietyModel>> UpdateAsync(Guid id, [FromBody] UpdateVarietyPayload payload, CancellationToken cancellationToken)
  {
    VarietyModel? variety = await _varietyService.UpdateAsync(id, payload, cancellationToken);
    return variety is null ? NotFound() : Ok(variety);
  }

  private ActionResult<VarietyModel> ToActionResult(CreateOrReplaceVarietyResult result)
  {
    VarietyModel variety = result.Variety;
    if (result.Created)
    {
      Uri location = new($"{HttpContext.GetBaseUri()}/varieties/{variety.Id}", UriKind.Absolute);
      return Created(location, variety);
    }
    return Ok(variety);
  }
}
