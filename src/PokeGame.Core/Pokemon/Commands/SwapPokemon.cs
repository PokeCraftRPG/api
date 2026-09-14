using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Rosters;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon.Commands;

internal record SwapPokemonCommand(SwapPokemonPayload Payload) : ICommand;

internal class SwapPokemonCommandHandler : ICommandHandler<SwapPokemonCommand, Unit>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly IRosterRepository _rosterRepository;

  public SwapPokemonCommandHandler(
    IContext context,
    IPermissionService permissionService,
    IPokemonRepository pokemonRepository,
    IRosterRepository rosterRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _pokemonRepository = pokemonRepository;
    _rosterRepository = rosterRepository;
  }

  public async Task<Unit> HandleAsync(SwapPokemonCommand command, CancellationToken cancellationToken)
  {
    SwapPokemonPayload payload = command.Payload;
    payload.Validate();

    ActorId? actorId = _context.ActorId;
    WorldId worldId = _context.WorldId;

    PokemonId[] pokemonIds = payload.PokemonIds.Select(entityId => new PokemonId(worldId, entityId)).ToArray();
    Dictionary<PokemonId, Specimen> specimens = (await _pokemonRepository.LoadAsync(pokemonIds, cancellationToken)).ToDictionary(x => x.Id, x => x);

    Specimen source = specimens.GetValueOrDefault(pokemonIds[0]) ?? throw new EntityNotFoundException(pokemonIds[0], nameof(payload.PokemonIds));
    await _permissionService.CheckAsync(Actions.Swap, source, cancellationToken);
    PokemonOwnership sourceOwnership = source.Ownership ?? throw new PokemonHasNoOwnerException(source);

    Specimen target = specimens.GetValueOrDefault(pokemonIds[1]) ?? throw new EntityNotFoundException(pokemonIds[1], nameof(payload.PokemonIds));
    await _permissionService.CheckAsync(Actions.Swap, target, cancellationToken);
    PokemonOwnership targetOwnership = target.Ownership ?? throw new PokemonHasNoOwnerException(target);

    if (sourceOwnership.TrainerId != targetOwnership.TrainerId)
    {
      throw new PokemonSwapRequiresSameOwnerException(source, target);
    }

    RosterId rosterId = new(sourceOwnership.TrainerId);
    Roster roster = await _rosterRepository.LoadAsync(rosterId, cancellationToken)
      ?? throw new InvalidOperationException($"The trainer 'Id={rosterId.TrainerId}' roster was not loaded.");
    await _permissionService.CheckAsync(Actions.Update, roster, cancellationToken);

    roster.Swap(source, target, actorId);

    await _rosterRepository.SaveAsync(roster, cancellationToken);

    return Unit.Value;
  }
}
