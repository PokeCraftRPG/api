using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Filters;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
[Authorize]
[RequireWorld]
[Route("pokemon")]
public class PokemonController : ControllerBase
{
  private const string GetByIdRoute = "GetPokemon";

  private readonly IPokemonService _pokemonService;

  public PokemonController(IPokemonService pokemonService)
  {
    _pokemonService = pokemonService;
  }

  [HttpPost("{id}/catch")]
  public async Task<ActionResult<PokemonDto>> CatchAsync(Guid id, [FromBody] CatchPokemonPayload payload, CancellationToken cancellationToken)
  {
    PokemonDto? pokemon = await _pokemonService.CatchAsync(id, payload, cancellationToken);
    return pokemon is null ? NotFound() : Ok(pokemon);
  }

  [HttpPut("{pokemonId}/form/{formId}")]
  public async Task<ActionResult<PokemonDto>> ChangeFormAsync(Guid pokemonId, Guid formId, CancellationToken cancellationToken)
  {
    PokemonDto? pokemon = await _pokemonService.ChangeFormAsync(pokemonId, formId, cancellationToken);
    return pokemon is null ? NotFound() : Ok(pokemon);
  }

  [HttpPost]
  public async Task<ActionResult<PokemonDto>> CreateAsync([FromBody] CreatePokemonPayload payload, CancellationToken cancellationToken)
  {
    PokemonDto pokemon = await _pokemonService.CreateAsync(payload, cancellationToken);
    return CreatedAtRoute(GetByIdRoute, new { id = pokemon.Id }, pokemon);
  }

  [HttpGet("{id}", Name = GetByIdRoute)]
  public async Task<ActionResult<PokemonDto>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    PokemonDto? pokemon = await _pokemonService.ReadAsync(id, key: null, cancellationToken);
    return pokemon is null ? NotFound() : Ok(pokemon);
  }

  [HttpGet("key:{key}")]
  public async Task<ActionResult<PokemonDto>> ReadAsync(string key, CancellationToken cancellationToken)
  {
    PokemonDto? pokemon = await _pokemonService.ReadAsync(id: null, key, cancellationToken);
    return pokemon is null ? NotFound() : Ok(pokemon);
  }

  [HttpPost("{id}/receive")]
  public async Task<ActionResult<PokemonDto>> ReceiveAsync(Guid id, [FromBody] ReceivePokemonPayload payload, CancellationToken cancellationToken)
  {
    PokemonDto? pokemon = await _pokemonService.ReceiveAsync(id, payload, cancellationToken);
    return pokemon is null ? NotFound() : Ok(pokemon);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<PokemonDto>> UpdateAsync(Guid id, [FromBody] UpdatePokemonPayload payload, CancellationToken cancellationToken)
  {
    PokemonDto? pokemon = await _pokemonService.UpdateAsync(id, payload, cancellationToken);
    return pokemon is null ? NotFound() : Ok(pokemon);
  }
}
