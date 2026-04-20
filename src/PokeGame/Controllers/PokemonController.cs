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

  [HttpPatch("{id}/catch")]
  public async Task<ActionResult<PokemonModel>> CatchAsync(Guid id, [FromBody] CatchPokemonPayload payload, CancellationToken cancellationToken)
  {
    PokemonModel? pokemon = await _pokemonService.CatchAsync(id, payload, cancellationToken);
    return pokemon is null ? NotFound() : Ok(pokemon);
  }

  [HttpPatch("{id}/form")]
  public async Task<ActionResult<PokemonModel>> ChangeFormAsync(Guid id, [FromBody] ChangePokemonFormPayload payload, CancellationToken cancellationToken)
  {
    PokemonModel? pokemon = await _pokemonService.ChangeFormAsync(id, payload, cancellationToken);
    return pokemon is null ? NotFound() : Ok(pokemon);
  }

  [HttpPost]
  public async Task<ActionResult<PokemonModel>> CreateAsync([FromBody] CreatePokemonPayload payload, CancellationToken cancellationToken)
  {
    PokemonModel pokemon = await _pokemonService.CreateAsync(payload, cancellationToken);
    Uri location = new($"{HttpContext.GetBaseUri()}/pokemon/{pokemon.Id}", UriKind.Absolute);
    return Created(location, pokemon);
  }

  [HttpPatch("{id}/deposit")]
  public async Task<ActionResult<PokemonModel>> DepositAsync(Guid id, CancellationToken cancellationToken)
  {
    PokemonModel? pokemon = await _pokemonService.DepositAsync(id, cancellationToken);
    return pokemon is null ? NotFound() : Ok(pokemon);
  }

  [HttpPatch("{id}/gift")]
  public async Task<ActionResult<PokemonModel>> GiftAsync(Guid id, [FromBody] GiftPokemonPayload payload, CancellationToken cancellationToken)
  {
    PokemonModel? pokemon = await _pokemonService.GiftAsync(id, payload, cancellationToken);
    return pokemon is null ? NotFound() : Ok(pokemon);
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

  [HttpPatch("{id}/receive")]
  public async Task<ActionResult<PokemonModel>> ReceiveAsync(Guid id, [FromBody] ReceivePokemonPayload payload, CancellationToken cancellationToken)
  {
    PokemonModel? pokemon = await _pokemonService.ReceiveAsync(id, payload, cancellationToken);
    return pokemon is null ? NotFound() : Ok(pokemon);
  }

  [HttpPatch("{id}/release")]
  public async Task<ActionResult<PokemonModel>> ReleaseAsync(Guid id, CancellationToken cancellationToken)
  {
    PokemonModel? pokemon = await _pokemonService.ReleaseAsync(id, cancellationToken);
    return pokemon is null ? NotFound() : Ok(pokemon);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<PokemonModel>> UpdateAsync(Guid id, [FromBody] UpdatePokemonPayload payload, CancellationToken cancellationToken)
  {
    PokemonModel? pokemon = await _pokemonService.UpdateAsync(id, payload, cancellationToken);
    return pokemon is null ? NotFound() : Ok(pokemon);
  }

  [HttpPatch("{id}/withdraw")]
  public async Task<ActionResult<PokemonModel>> WithdrawAsync(Guid id, CancellationToken cancellationToken)
  {
    PokemonModel? pokemon = await _pokemonService.WithdrawAsync(id, cancellationToken);
    return pokemon is null ? NotFound() : Ok(pokemon);
  }
}
