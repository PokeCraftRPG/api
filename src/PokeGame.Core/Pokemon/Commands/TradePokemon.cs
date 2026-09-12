using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Regions;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon.Commands;

internal record TradePokemonCommand(TradePokemonPayload Payload) : ICommand;

internal class TradePokemonCommandHandler : ICommandHandler<TradePokemonCommand, Unit>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonRepository _pokemonRepository;

  public TradePokemonCommandHandler(IContext context, IPermissionService permissionService, IPokemonRepository pokemonRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _pokemonRepository = pokemonRepository;
  }

  public async Task<Unit> HandleAsync(TradePokemonCommand command, CancellationToken cancellationToken)
  {
    TradePokemonPayload payload = command.Payload;
    payload.Validate();

    WorldId worldId = _context.WorldId;
    PokemonId[] pokemonIds = payload.PokemonIds.Select(entityId => new PokemonId(worldId, entityId)).ToArray();
    Dictionary<PokemonId, Specimen> specimens = (await _pokemonRepository.LoadAsync(pokemonIds, cancellationToken)).ToDictionary(x => x.Id, x => x);

    Specimen source = specimens.GetValueOrDefault(pokemonIds[0]) ?? throw new EntityNotFoundException(pokemonIds[0], nameof(payload.PokemonIds));
    Specimen target = specimens.GetValueOrDefault(pokemonIds[1]) ?? throw new EntityNotFoundException(pokemonIds[1], nameof(payload.PokemonIds));
    await _permissionService.CheckAsync(Actions.Update, source, cancellationToken);
    await _permissionService.CheckAsync(Actions.Update, target, cancellationToken);

    Location location = new(payload.Location);
    source.Trade(target, location, _context.ActorId);

    await _pokemonRepository.SaveAsync([source, target], cancellationToken);

    return Unit.Value;
  }
}

// TODO(fpion): PokéDex
// TODO(fpion): Position
