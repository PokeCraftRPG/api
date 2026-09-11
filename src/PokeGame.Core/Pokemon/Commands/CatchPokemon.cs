using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Inventory;
using PokeGame.Core.Items;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Regions;
using PokeGame.Core.Trainers;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon.Commands;

internal record CatchPokemonCommand(Guid PokemonId, CatchPokemonPayload Payload) : ICommand<PokemonDto?>;

internal class CatchPokemonCommandHandler : ICommandHandler<CatchPokemonCommand, PokemonDto?>
{
  private readonly IContext _context;
  private readonly IInventoryRepository _inventoryRepository;
  private readonly IItemRepository _itemRepository;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonQuerier _pokemonQuerier;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly ITrainerRepository _trainerRepository;

  public CatchPokemonCommandHandler(
    IContext context,
    IInventoryRepository inventoryRepository,
    IItemRepository itemRepository,
    IPermissionService permissionService,
    IPokemonQuerier pokemonQuerier,
    IPokemonRepository pokemonRepository,
    ITrainerRepository trainerRepository)
  {
    _context = context;
    _inventoryRepository = inventoryRepository;
    _itemRepository = itemRepository;
    _permissionService = permissionService;
    _pokemonQuerier = pokemonQuerier;
    _pokemonRepository = pokemonRepository;
    _trainerRepository = trainerRepository;
  }

  public async Task<PokemonDto?> HandleAsync(CatchPokemonCommand command, CancellationToken cancellationToken)
  {
    CatchPokemonPayload payload = command.Payload;
    payload.Validate();

    ActorId? actorId = _context.ActorId;
    WorldId worldId = _context.WorldId;

    PokemonId pokemonId = new(worldId, command.PokemonId);
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

    InventoryId inventoryId = new(trainer.Id);
    TrainerInventory inventory = await _inventoryRepository.LoadAsync(inventoryId, cancellationToken) ?? new(trainer);
    await _permissionService.CheckAsync(Actions.Update, inventory, cancellationToken);

    Location location = new(payload.Location);

    specimen.Catch(trainer, pokeBall, location, actorId);
    inventory.AdjustQuantity(pokeBall, delta: -1, actorId);

    await _pokemonRepository.SaveAsync(specimen, cancellationToken);
    await _inventoryRepository.SaveAsync(inventory, cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}

// TODO(fpion): PokéDex
// TODO(fpion): Position
