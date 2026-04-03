using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Moves.Models;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Extensions;

namespace PokeGame.Controllers;

[ApiController]
[Authorize]
[Route("pokemon")]
public class PokemonController : ControllerBase
{
  private readonly IPokemonService _pokemonService;

  public PokemonController(IPokemonService pokemonService)
  {
    _pokemonService = pokemonService;
  }

  [HttpPost]
  public async Task<ActionResult<PokemonModel>> CreateAsync([FromBody] CreatePokemonPayload payload, CancellationToken cancellationToken)
  {
    PokemonModel pokemon = await _pokemonService.CreateAsync(payload, cancellationToken);
    Uri location = new($"{HttpContext.GetBaseUri()}/pokemon/{pokemon.Id}", UriKind.Absolute);
    return Created(location, pokemon);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<MoveModel>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    PokemonModel? pokemon = await _pokemonService.ReadAsync(id, key: null, cancellationToken);
    return pokemon is null ? NotFound() : Ok(pokemon);
  }

  [HttpGet("key:{key}")]
  public async Task<ActionResult<MoveModel>> ReadAsync(string key, CancellationToken cancellationToken)
  {
    PokemonModel? pokemon = await _pokemonService.ReadAsync(id: null, key, cancellationToken);
    return pokemon is null ? NotFound() : Ok(pokemon);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<PokemonModel>> UpdateAsync(Guid id, [FromBody] UpdatePokemonPayload payload, CancellationToken cancellationToken)
  {
    PokemonModel? pokemon = await _pokemonService.UpdateAsync(id, payload, cancellationToken);
    return pokemon is null ? NotFound() : Ok(pokemon);
  }
}
