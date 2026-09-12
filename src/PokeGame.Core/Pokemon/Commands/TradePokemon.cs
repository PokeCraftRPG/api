using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Messaging;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Regions;
using PokeGame.Core.Rosters;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon.Commands;

internal record TradePokemonCommand(TradePokemonPayload Payload) : ICommand;

internal class TradePokemonCommandHandler : ICommandHandler<TradePokemonCommand, Unit>
{
  private readonly IContext _context;
  private readonly IMessagingManager _messagingManager;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly IRosterRepository _rosterRepository;

  public TradePokemonCommandHandler(
    IContext context,
    IMessagingManager messagingManager,
    IPermissionService permissionService,
    IPokemonRepository pokemonRepository,
    IRosterRepository rosterRepository)
  {
    _context = context;
    _messagingManager = messagingManager;
    _permissionService = permissionService;
    _pokemonRepository = pokemonRepository;
    _rosterRepository = rosterRepository;
  }

  public async Task<Unit> HandleAsync(TradePokemonCommand command, CancellationToken cancellationToken)
  {
    TradePokemonPayload payload = command.Payload;
    payload.Validate();

    ActorId? actorId = _context.ActorId;
    WorldId worldId = _context.WorldId;

    PokemonId[] pokemonIds = payload.PokemonIds.Select(entityId => new PokemonId(worldId, entityId)).ToArray();
    Dictionary<PokemonId, Specimen> specimens = (await _pokemonRepository.LoadAsync(pokemonIds, cancellationToken)).ToDictionary(x => x.Id, x => x);

    Specimen source = specimens.GetValueOrDefault(pokemonIds[0]) ?? throw new EntityNotFoundException(pokemonIds[0], nameof(payload.PokemonIds));
    await _permissionService.CheckAsync(Actions.Update, source, cancellationToken);

    Specimen target = specimens.GetValueOrDefault(pokemonIds[1]) ?? throw new EntityNotFoundException(pokemonIds[1], nameof(payload.PokemonIds));
    await _permissionService.CheckAsync(Actions.Update, target, cancellationToken);

    RosterId sourceRosterId = new(source.Ownership?.TrainerId ?? throw new PokemonHasNoOwnerException(source));
    RosterId targetRosterId = new(target.Ownership?.TrainerId ?? throw new PokemonHasNoOwnerException(target));
    HashSet<RosterId> rosterIds = new([sourceRosterId, targetRosterId]);
    Dictionary<RosterId, Roster> rosters = (await _rosterRepository.LoadAsync(rosterIds, cancellationToken)).ToDictionary(x => x.Id, x => x);

    Roster sourceRoster = rosters.GetValueOrDefault(sourceRosterId) ?? new(sourceRosterId);
    await _permissionService.CheckAsync(Actions.Update, sourceRoster, cancellationToken);

    Roster targetRoster = rosters.GetValueOrDefault(targetRosterId) ?? new(targetRosterId);
    await _permissionService.CheckAsync(Actions.Update, targetRoster, cancellationToken);

    Location location = new(payload.Location);

    source.Trade(target, location, actorId);
    sourceRoster.Swap(source, target, actorId);
    targetRoster.Swap(target, source, actorId);

    await _pokemonRepository.SaveAsync([source, target], cancellationToken);
    await _rosterRepository.SaveAsync([sourceRoster, targetRoster], cancellationToken);

    IEnumerable<PokemonAcquired> acquired = new Specimen[] { source, target }.Select(PokemonAcquired.From);
    await _messagingManager.PublishAsync(acquired, cancellationToken);

    return Unit.Value;
  }
}
