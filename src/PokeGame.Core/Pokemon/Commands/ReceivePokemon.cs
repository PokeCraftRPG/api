using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Items;
using PokeGame.Core.Messaging;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Regions;
using PokeGame.Core.Rosters;
using PokeGame.Core.Trainers;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon.Commands;

internal record ReceivePokemonCommand(Guid Id, ReceivePokemonPayload Payload) : ICommand<PokemonDto?>;

internal class ReceivePokemonCommandHandler : ICommandHandler<ReceivePokemonCommand, PokemonDto?>
{
  private readonly IContext _context;
  private readonly IItemRepository _itemRepository;
  private readonly IMessagingManager _messagingManager;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonQuerier _pokemonQuerier;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly IRosterRepository _rosterRepository;
  private readonly ITrainerRepository _trainerRepository;

  public ReceivePokemonCommandHandler(
    IContext context,
    IItemRepository itemRepository,
    IMessagingManager messagingManager,
    IPermissionService permissionService,
    IPokemonQuerier pokemonQuerier,
    IPokemonRepository pokemonRepository,
    IRosterRepository rosterRepository,
    ITrainerRepository trainerRepository)
  {
    _context = context;
    _itemRepository = itemRepository;
    _messagingManager = messagingManager;
    _permissionService = permissionService;
    _pokemonQuerier = pokemonQuerier;
    _pokemonRepository = pokemonRepository;
    _rosterRepository = rosterRepository;
    _trainerRepository = trainerRepository;
  }

  public async Task<PokemonDto?> HandleAsync(ReceivePokemonCommand command, CancellationToken cancellationToken)
  {
    ReceivePokemonPayload payload = command.Payload;
    payload.Validate();

    ActorId? actorId = _context.ActorId;
    WorldId worldId = _context.WorldId;

    PokemonId pokemonId = new(worldId, command.Id);
    Specimen? specimen = await _pokemonRepository.LoadAsync(pokemonId, cancellationToken);
    if (specimen is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, specimen, cancellationToken);

    TrainerId trainerId = new(worldId, payload.TrainerId);
    Trainer trainer = await _trainerRepository.LoadAsync(trainerId, cancellationToken) ?? throw new EntityNotFoundException(trainerId, nameof(payload.TrainerId));

    ItemId pokeBallId = new(worldId, payload.PokeBallId);
    Item pokeBall = await _itemRepository.LoadAsync(pokeBallId, cancellationToken) ?? throw new EntityNotFoundException(pokeBallId, nameof(payload.PokeBallId));

    List<Roster> rosters = new(capacity: 2);
    Roster? sourceRoster = null;
    if (specimen.Ownership is not null)
    {
      RosterId sourceRosterId = new(specimen.Ownership.TrainerId);
      sourceRoster = await _rosterRepository.LoadAsync(sourceRosterId, cancellationToken);
      if (sourceRoster is not null)
      {
        await _permissionService.CheckAsync(Actions.Update, sourceRoster, cancellationToken);
        rosters.Add(sourceRoster);
      }
    }

    RosterId targetRosterId = new(trainer.Id);
    Roster targetRoster = await _rosterRepository.LoadAsync(targetRosterId, cancellationToken) ?? new(trainer);
    await _permissionService.CheckAsync(Actions.Update, targetRoster, cancellationToken);
    rosters.Add(targetRoster);

    Location location = new(payload.Location);

    specimen.Receive(trainer, pokeBall, location, actorId);
    sourceRoster?.Remove(specimen, actorId);
    targetRoster.Add(specimen, trainer, actorId);

    await _pokemonRepository.SaveAsync(specimen, cancellationToken);
    await _rosterRepository.SaveAsync(rosters, cancellationToken);

    PokemonAcquired acquired = PokemonAcquired.From(specimen);
    await _messagingManager.PublishAsync(acquired, cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
