using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Abilities;
using PokeGame.Core.Abilities.Models;
using PokeGame.Extensions;

namespace PokeGame.Controllers;

[ApiController]
[Authorize]
[Route("abilities")]
public class AbilityController : ControllerBase
{
  private readonly IAbilityService _abilityService;

  public AbilityController(IAbilityService abilityService)
  {
    _abilityService = abilityService;
  }

  [HttpPost]
  public async Task<ActionResult<AbilityModel>> CreateAsync([FromBody] CreateOrReplaceAbilityPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceAbilityResult result = await _abilityService.CreateOrReplaceAsync(payload, id: null, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<AbilityModel>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    AbilityModel? ability = await _abilityService.ReadAsync(id, slug: null, cancellationToken);
    return ability is null ? NotFound() : Ok(ability);
  }

  [HttpGet("slug:{slug}")]
  public async Task<ActionResult<AbilityModel>> ReadAsync(string slug, CancellationToken cancellationToken)
  {
    AbilityModel? ability = await _abilityService.ReadAsync(id: null, slug, cancellationToken);
    return ability is null ? NotFound() : Ok(ability);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<AbilityModel>> ReplaceAsync(Guid id, [FromBody] CreateOrReplaceAbilityPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceAbilityResult result = await _abilityService.CreateOrReplaceAsync(payload, id, cancellationToken);
    return ToActionResult(result);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<AbilityModel>> UpdateAsync(Guid id, [FromBody] UpdateAbilityPayload payload, CancellationToken cancellationToken)
  {
    AbilityModel? ability = await _abilityService.UpdateAsync(id, payload, cancellationToken);
    return ability is null ? NotFound() : Ok(ability);
  }

  private ActionResult<AbilityModel> ToActionResult(CreateOrReplaceAbilityResult result)
  {
    AbilityModel ability = result.Ability;
    if (result.Created)
    {
      Uri location = new($"{HttpContext.GetBaseUri()}/abilities/{ability.Id}", UriKind.Absolute);
      return Created(location, ability);
    }
    return Ok(ability);
  }
}
