using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Filters;
using PokeGame.Core.Species;
using PokeGame.Core.Species.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
[Authorize]
[RequireWorld]
[Route("/species/{speciesId}/regions")]
public class RegionalNumberController : ControllerBase
{
  private readonly ISpeciesService _speciesService;

  public RegionalNumberController(ISpeciesService speciesService)
  {
    _speciesService = speciesService;
  }

  [HttpGet("{regionId}")]
  public async Task<ActionResult<RegionalNumberDto>> ReadAsync(Guid speciesId, Guid regionId, CancellationToken cancellationToken)
  {
    SpeciesDto? species = await _speciesService.ReadAsync(speciesId, number: null, key: null, cancellationToken);
    if (species is null)
    {
      return NotFound();
    }

    RegionalNumberDto? regionalNumber = species.RegionalNumbers.SingleOrDefault(x => x.Region.Id == regionId);
    return regionalNumber is null ? NotFound() : Ok(regionalNumber);
  }

  [HttpGet("/regions/{region}/species/{number}")]
  public async Task<ActionResult<SpeciesDto>> ReadSpeciesAsync(string region, int number, CancellationToken cancellationToken)
  {
    SpeciesDto? species = await _speciesService.ReadAsync(region, number, cancellationToken);
    return species is null ? NotFound() : Ok(species);
  }

  [HttpDelete("{regionId}")]
  public async Task<ActionResult<SpeciesDto>> RemoveAsync(Guid speciesId, Guid regionId, CancellationToken cancellationToken)
  {
    SpeciesDto? species = await _speciesService.RemoveRegionalNumberAsync(speciesId, regionId, cancellationToken);
    return species is null ? NotFound() : Ok(species);
  }

  [HttpGet]
  public async Task<ActionResult<SearchResults<RegionalNumberDto>>> SearchAsync(Guid speciesId, CancellationToken cancellationToken)
  {
    SpeciesDto? species = await _speciesService.ReadAsync(speciesId, number: null, key: null, cancellationToken);
    if (species is null)
    {
      return NotFound();
    }

    SearchResults<RegionalNumberDto> results = new(species.RegionalNumbers.OrderBy(x => x.Region.Name ?? x.Region.Key));
    return Ok(results);
  }

  [HttpPut("{regionId}")]
  public async Task<ActionResult<SpeciesDto>> SetAsync(Guid speciesId, Guid regionId, SetRegionalNumberPayload payload, CancellationToken cancellationToken)
  {
    SpeciesDto species = await _speciesService.SetRegionalNumberAsync(speciesId, regionId, payload, cancellationToken);
    return Ok(species);
  }
}
