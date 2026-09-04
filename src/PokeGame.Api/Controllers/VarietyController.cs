using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Extensions;
using PokeGame.Api.Filters;
using PokeGame.Api.Models.Variety;
using PokeGame.Core.Varieties;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
[Authorize]
[RequireWorld]
[Route("varieties")]
public class VarietyController : ControllerBase
{
  private readonly IVarietyService _varietyService;

  public VarietyController(IVarietyService varietyService)
  {
    _varietyService = varietyService;
  }

  [HttpPost]
  public async Task<ActionResult<VarietyDto>> CreateAsync([FromBody] CreateOrReplaceVarietyPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceVarietyResult result = await _varietyService.CreateOrReplaceAsync(payload, id: null, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<VarietyDto>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    VarietyDto? variety = await _varietyService.ReadAsync(id, key: null, cancellationToken);
    return variety is null ? NotFound() : Ok(variety);
  }

  [HttpGet("key:{key}")]
  public async Task<ActionResult<VarietyDto>> ReadAsync(string key, CancellationToken cancellationToken)
  {
    VarietyDto? variety = await _varietyService.ReadAsync(id: null, key, cancellationToken);
    return variety is null ? NotFound() : Ok(variety);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<VarietyDto>> ReplaceAsync(Guid id, [FromBody] CreateOrReplaceVarietyPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceVarietyResult result = await _varietyService.CreateOrReplaceAsync(payload, id, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet]
  public async Task<ActionResult<SearchResults<VarietyDto>>> SearchAsync([FromQuery] SearchVarietiesParameters parameters, CancellationToken cancellationToken)
  {
    SearchVarietiesPayload payload = parameters.ToPayload();
    SearchResults<VarietyDto> varieties = await _varietyService.SearchAsync(payload, cancellationToken);
    return Ok(varieties);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<VarietyDto>> UpdateAsync(Guid id, [FromBody] UpdateVarietyPayload payload, CancellationToken cancellationToken)
  {
    VarietyDto? variety = await _varietyService.UpdateAsync(id, payload, cancellationToken);
    return variety is null ? NotFound() : Ok(variety);
  }

  private ActionResult<VarietyDto> ToActionResult(CreateOrReplaceVarietyResult result)
  {
    VarietyDto variety = result.Variety;
    if (result.Created)
    {
      Uri location = new($"{HttpContext.GetBaseUrl()}/varieties/{variety.Id}", UriKind.Absolute);
      return Created(location, variety);
    }
    return Ok(variety);
  }
}
