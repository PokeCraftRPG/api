using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Filters;
using PokeGame.Api.Models.Ability;
using PokeGame.Core.Abilities;
using PokeGame.Core.Abilities.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
[Authorize]
[RequireWorld]
[Route("abilities")]
public class AbilityController : ControllerBase
{
  private const string GetByIdRoute = "GetAbility";

  private readonly IAbilityService _abilityService;

  public AbilityController(IAbilityService abilityService)
  {
    _abilityService = abilityService;
  }

  [HttpPost]
  public async Task<ActionResult<AbilityDto>> CreateAsync([FromBody] CreateOrReplaceAbilityPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceAbilityResult result = await _abilityService.CreateOrReplaceAsync(payload, id: null, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet("{id}", Name = GetByIdRoute)]
  public async Task<ActionResult<AbilityDto>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    AbilityDto? ability = await _abilityService.ReadAsync(id, key: null, cancellationToken);
    return ability is null ? NotFound() : Ok(ability);
  }

  [HttpGet("key:{key}")]
  public async Task<ActionResult<AbilityDto>> ReadAsync(string key, CancellationToken cancellationToken)
  {
    AbilityDto? ability = await _abilityService.ReadAsync(id: null, key, cancellationToken);
    return ability is null ? NotFound() : Ok(ability);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<AbilityDto>> ReplaceAsync(Guid id, [FromBody] CreateOrReplaceAbilityPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceAbilityResult result = await _abilityService.CreateOrReplaceAsync(payload, id, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet]
  public async Task<ActionResult<SearchResults<AbilityDto>>> SearchAsync([FromQuery] SearchAbilitiesParameters parameters, CancellationToken cancellationToken)
  {
    SearchAbilitiesPayload payload = parameters.ToPayload();
    SearchResults<AbilityDto> abilities = await _abilityService.SearchAsync(payload, cancellationToken);
    return Ok(abilities);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<AbilityDto>> UpdateAsync(Guid id, [FromBody] UpdateAbilityPayload payload, CancellationToken cancellationToken)
  {
    AbilityDto? ability = await _abilityService.UpdateAsync(id, payload, cancellationToken);
    return ability is null ? NotFound() : Ok(ability);
  }

  private ActionResult<AbilityDto> ToActionResult(CreateOrReplaceAbilityResult result) => result.Created
    ? CreatedAtRoute(GetByIdRoute, new { id = result.Ability.Id }, result.Ability)
    : Ok(result.Ability);
}
