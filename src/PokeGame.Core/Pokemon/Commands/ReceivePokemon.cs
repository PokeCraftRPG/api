using Logitar.CQRS;
using PokeGame.Core.Items;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Regions;
using PokeGame.Core.Trainers;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon.Commands;

internal record ReceivePokemonCommand(Guid Id, ReceivePokemonPayload Payload) : ICommand<PokemonDto?>;

internal class ReceivePokemonCommandHandler : ICommandHandler<ReceivePokemonCommand, PokemonDto?>
{
  private readonly IContext _context;
  private readonly IItemRepository _itemRepository;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonQuerier _pokemonQuerier;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly ITrainerRepository _trainerRepository;

  public ReceivePokemonCommandHandler(
    IContext context,
    IItemRepository itemRepository,
    IPermissionService permissionService,
    IPokemonQuerier pokemonQuerier,
    IPokemonRepository pokemonRepository,
    ITrainerRepository trainerRepository)
  {
    _context = context;
    _itemRepository = itemRepository;
    _permissionService = permissionService;
    _pokemonQuerier = pokemonQuerier;
    _pokemonRepository = pokemonRepository;
    _trainerRepository = trainerRepository;
  }

  public async Task<PokemonDto?> HandleAsync(ReceivePokemonCommand command, CancellationToken cancellationToken)
  {
    ReceivePokemonPayload payload = command.Payload;
    payload.Validate();

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

    Location location = new(payload.Location);

    specimen.Receive(trainer, pokeBall, location, _context.ActorId);

    await _pokemonRepository.SaveAsync(specimen, cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}

// TODO(fpion): PokéDex
// TODO(fpion): Position
