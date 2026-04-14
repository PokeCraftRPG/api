using Logitar.CQRS;
using PokeGame.Core.Items;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Regions;
using PokeGame.Core.Rosters;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Pokemon.Commands;

internal record ReceivePokemonCommand(Guid Id, ReceivePokemonPayload Payload) : ICommand<PokemonModel?>;

internal class ReceivePokemonCommandHandler : ICommandHandler<ReceivePokemonCommand, PokemonModel?>
{
  private readonly IContext _context;
  private readonly IItemManager _itemManager;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonQuerier _pokemonQuerier;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly IRosterRepository _rosterRepository;
  private readonly ITrainerManager _trainerManager;

  public ReceivePokemonCommandHandler(
    IContext context,
    IItemManager itemManager,
    IPermissionService permissionService,
    IPokemonQuerier pokemonQuerier,
    IPokemonRepository pokemonRepository,
    IRosterRepository rosterRepository,
    ITrainerManager trainerManager)
  {
    _context = context;
    _itemManager = itemManager;
    _permissionService = permissionService;
    _pokemonQuerier = pokemonQuerier;
    _pokemonRepository = pokemonRepository;
    _rosterRepository = rosterRepository;
    _trainerManager = trainerManager;
  }

  public async Task<PokemonModel?> HandleAsync(ReceivePokemonCommand command, CancellationToken cancellationToken)
  {
    ReceivePokemonPayload payload = command.Payload;
    payload.Validate();

    PokemonId pokemonId = new(_context.WorldId, command.Id);
    Specimen? specimen = await _pokemonRepository.LoadAsync(pokemonId, cancellationToken);
    if (specimen is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Receive, specimen, cancellationToken);

    Trainer trainer = await _trainerManager.FindAsync(payload.Trainer, nameof(payload.Trainer), cancellationToken);
    Item pokeBall = await _itemManager.FindAsync(payload.PokeBall, nameof(payload.PokeBall), cancellationToken);

    UserId userId = _context.UserId;

    List<Roster> rosters = new(capacity: 2);
    if (specimen.Ownership is not null)
    {
      RosterId rosterId = new(specimen.Ownership.TrainerId);
      Roster? previousRoster = await _rosterRepository.LoadAsync(rosterId, cancellationToken);
      if (previousRoster is not null)
      {
        previousRoster.Remove(specimen, userId);
        rosters.Add(previousRoster);
      }
    }

    Location location = new(payload.Location);
    specimen.Receive(trainer, pokeBall, location, _context.UserId);

    Roster roster = await _rosterRepository.LoadAsync(trainer, cancellationToken);
    roster.Add(specimen, userId);
    rosters.Add(roster);

    await _pokemonRepository.SaveAsync(specimen, cancellationToken);
    await _rosterRepository.SaveAsync(rosters, cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
