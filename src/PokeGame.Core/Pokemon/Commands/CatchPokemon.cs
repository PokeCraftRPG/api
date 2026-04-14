using Logitar.CQRS;
using PokeGame.Core.Inventory;
using PokeGame.Core.Items;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Regions;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Pokemon.Commands;

internal record CatchPokemonCommand(Guid Id, CatchPokemonPayload Payload) : ICommand<PokemonModel?>;

internal class CatchPokemonCommandHandler : ICommandHandler<CatchPokemonCommand, PokemonModel?>
{
  private readonly IContext _context;
  private readonly IInventoryRepository _inventoryRepository;
  private readonly IItemManager _itemManager;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonQuerier _pokemonQuerier;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly ITrainerManager _trainerManager;

  public CatchPokemonCommandHandler(
    IContext context,
    IInventoryRepository inventoryRepository,
    IItemManager itemManager,
    IPermissionService permissionService,
    IPokemonQuerier pokemonQuerier,
    IPokemonRepository pokemonRepository,
    ITrainerManager trainerManager)
  {
    _context = context;
    _inventoryRepository = inventoryRepository;
    _itemManager = itemManager;
    _permissionService = permissionService;
    _pokemonQuerier = pokemonQuerier;
    _pokemonRepository = pokemonRepository;
    _trainerManager = trainerManager;
  }

  public async Task<PokemonModel?> HandleAsync(CatchPokemonCommand command, CancellationToken cancellationToken)
  {
    CatchPokemonPayload payload = command.Payload;
    payload.Validate();

    SpecimenId specimenId = new(_context.WorldId, command.Id);
    Specimen? specimen = await _pokemonRepository.LoadAsync(specimenId, cancellationToken);
    if (specimen is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Catch, specimen, cancellationToken);

    Trainer trainer = await _trainerManager.FindAsync(payload.Trainer, nameof(payload.Trainer), cancellationToken);
    Item pokeBall = await _itemManager.FindAsync(payload.PokeBall, nameof(payload.PokeBall), cancellationToken);

    InventoryAggregate inventory = await _inventoryRepository.LoadAsync(trainer, cancellationToken);
    inventory.EnsureQuantity(pokeBall, quantity: 1);
    inventory.Remove(pokeBall, quantity: 1, _context.UserId);

    Location location = new(payload.Location);
    specimen.Catch(trainer, pokeBall, location, _context.UserId);

    // TODO(fpion): update storage

    await _inventoryRepository.SaveAsync(inventory, cancellationToken);
    await _pokemonRepository.SaveAsync(specimen, cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
