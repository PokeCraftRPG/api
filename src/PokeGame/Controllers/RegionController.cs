using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Regions;
using PokeGame.Core.Regions.Models;
using PokeGame.Extensions;
using PokeGame.Models.Region;

namespace PokeGame.Controllers;

[ApiController]
[Authorize]
[Route("regions")]
public class RegionController : ControllerBase
{
  private readonly IRegionService _regionService;

  public RegionController(IRegionService regionService)
  {
    _regionService = regionService;
  }

  [HttpPost]
  public async Task<ActionResult<RegionModel>> CreateAsync([FromBody] CreateOrReplaceRegionPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceRegionResult result = await _regionService.CreateOrReplaceAsync(payload, id: null, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<RegionModel>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    RegionModel? region = await _regionService.ReadAsync(id, key: null, cancellationToken);
    return region is null ? NotFound() : Ok(region);
  }

  [HttpGet("key:{key}")]
  public async Task<ActionResult<RegionModel>> ReadAsync(string key, CancellationToken cancellationToken)
  {
    RegionModel? region = await _regionService.ReadAsync(id: null, key, cancellationToken);
    return region is null ? NotFound() : Ok(region);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<RegionModel>> ReplaceAsync(Guid id, [FromBody] CreateOrReplaceRegionPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceRegionResult result = await _regionService.CreateOrReplaceAsync(payload, id, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet]
  public async Task<ActionResult<SearchResults<RegionModel>>> SearchAsync([FromQuery] SearchRegionsParameters parameters, CancellationToken cancellationToken)
  {
    SearchRegionsPayload payload = parameters.ToPayload();
    SearchResults<RegionModel> regions = await _regionService.SearchAsync(payload, cancellationToken);
    return Ok(regions);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<RegionModel>> UpdateAsync(Guid id, [FromBody] UpdateRegionPayload payload, CancellationToken cancellationToken)
  {
    RegionModel? region = await _regionService.UpdateAsync(id, payload, cancellationToken);
    return region is null ? NotFound() : Ok(region);
  }

  private ActionResult<RegionModel> ToActionResult(CreateOrReplaceRegionResult result)
  {
    RegionModel region = result.Region;
    if (result.Created)
    {
      Uri location = new($"{HttpContext.GetBaseUri()}/regions/{region.Id}", UriKind.Absolute);
      return Created(location, region);
    }
    return Ok(region);
  }
}
