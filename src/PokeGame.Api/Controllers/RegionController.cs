using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Extensions;
using PokeGame.Api.Filters;
using PokeGame.Api.Models.Region;
using PokeGame.Core.Regions;
using PokeGame.Core.Regions.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
[Authorize]
[RequireWorld]
[Route("regions")]
public class RegionController : ControllerBase
{
  private readonly IRegionService _regionService;

  public RegionController(IRegionService regionService)
  {
    _regionService = regionService;
  }

  [HttpPost]
  public async Task<ActionResult<RegionDto>> CreateAsync([FromBody] CreateOrReplaceRegionPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceRegionResult result = await _regionService.CreateOrReplaceAsync(payload, id: null, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<RegionDto>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    RegionDto? region = await _regionService.ReadAsync(id, key: null, cancellationToken);
    return region is null ? NotFound() : Ok(region);
  }

  [HttpGet("key:{key}")]
  public async Task<ActionResult<RegionDto>> ReadAsync(string key, CancellationToken cancellationToken)
  {
    RegionDto? region = await _regionService.ReadAsync(id: null, key, cancellationToken);
    return region is null ? NotFound() : Ok(region);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<RegionDto>> ReplaceAsync(Guid id, [FromBody] CreateOrReplaceRegionPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceRegionResult result = await _regionService.CreateOrReplaceAsync(payload, id, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet]
  public async Task<ActionResult<SearchResults<RegionDto>>> SearchAsync([FromQuery] SearchRegionsParameters parameters, CancellationToken cancellationToken)
  {
    SearchRegionsPayload payload = parameters.ToPayload();
    SearchResults<RegionDto> regions = await _regionService.SearchAsync(payload, cancellationToken);
    return Ok(regions);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<RegionDto>> UpdateAsync(Guid id, [FromBody] UpdateRegionPayload payload, CancellationToken cancellationToken)
  {
    RegionDto? region = await _regionService.UpdateAsync(id, payload, cancellationToken);
    return region is null ? NotFound() : Ok(region);
  }

  private ActionResult<RegionDto> ToActionResult(CreateOrReplaceRegionResult result)
  {
    RegionDto region = result.Region;
    if (result.Created)
    {
      Uri location = new($"{HttpContext.GetBaseUrl()}/regions/{region.Id}", UriKind.Absolute);
      return Created(location, region);
    }
    return Ok(region);
  }
}
