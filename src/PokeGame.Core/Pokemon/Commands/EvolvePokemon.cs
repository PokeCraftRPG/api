using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Evolutions;
using PokeGame.Core.Forms;
using PokeGame.Core.Inventory;
using PokeGame.Core.Messaging;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Regions;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon.Commands;

internal record EvolvePokemonCommand(Guid Id, EvolvePokemonPayload Payload) : ICommand<PokemonDto?>;

internal class EvolvePokemonCommandHandler : ICommandHandler<EvolvePokemonCommand, PokemonDto?>
{
  private readonly IContext _context;
  private readonly IEvolutionRepository _evolutionRepository;
  private readonly IFormRepository _formRepository;
  private readonly IInventoryRepository _inventoryRepository;
  private readonly IMessagingManager _messagingManager;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonQuerier _pokemonQuerier;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly IVarietyRepository _varietyRepository;

  public EvolvePokemonCommandHandler(
    IContext context,
    IEvolutionRepository evolutionRepository,
    IFormRepository formRepository,
    IInventoryRepository inventoryRepository,
    IMessagingManager messagingManager,
    IPermissionService permissionService,
    IPokemonQuerier pokemonQuerier,
    IPokemonRepository pokemonRepository,
    IVarietyRepository varietyRepository)
  {
    _context = context;
    _evolutionRepository = evolutionRepository;
    _formRepository = formRepository;
    _inventoryRepository = inventoryRepository;
    _messagingManager = messagingManager;
    _permissionService = permissionService;
    _pokemonQuerier = pokemonQuerier;
    _pokemonRepository = pokemonRepository;
    _varietyRepository = varietyRepository;
  }

  public async Task<PokemonDto?> HandleAsync(EvolvePokemonCommand command, CancellationToken cancellationToken)
  {
    EvolvePokemonPayload payload = command.Payload;
    payload.Validate();

    ActorId? actorId = _context.ActorId;
    WorldId worldId = _context.WorldId;

    PokemonId pokemonId = new(worldId, command.Id);
    Specimen? specimen = await _pokemonRepository.LoadAsync(pokemonId, cancellationToken);
    if (specimen is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Evolve, specimen, cancellationToken);

    EvolutionId evolutionId = new(worldId, payload.EvolutionId);
    Evolution evolution = await _evolutionRepository.LoadAsync(evolutionId, cancellationToken)
      ?? throw new EntityNotFoundException(evolutionId, nameof(payload.EvolutionId));
    Form form = await _formRepository.LoadAsync(evolution.TargetId, cancellationToken)
      ?? throw new InvalidOperationException($"The form 'Id={evolution.TargetId}' was not loaded.");
    Variety variety = await _varietyRepository.LoadAsync(form.VarietyId, cancellationToken)
      ?? throw new InvalidOperationException($"The variety 'Id={form.VarietyId}' was not loaded.");

    TrainerInventory? inventory = null;
    if (evolution.Trigger == EvolutionTrigger.ItemUsed && evolution.ItemId.HasValue)
    {
      PokemonOwnership ownership = specimen.Ownership ?? throw new PokemonHasNoOwnerException(specimen);
      InventoryId inventoryId = new(ownership.TrainerId);
      inventory = await _inventoryRepository.LoadAsync(inventoryId, cancellationToken) ?? new(inventoryId);
      inventory.UseItem(evolution.ItemId.Value, actorId);
    }

    Location? location = Location.TryCreate(payload.Location);

    specimen.Evolve(evolution, form, variety, location, payload.TimeOfDay, actorId); // TODO(fpion): evolution moves

    await _pokemonRepository.SaveAsync(specimen, cancellationToken);
    if (inventory is not null)
    {
      await _inventoryRepository.SaveAsync(inventory, cancellationToken);
    }

    PokemonAcquired acquired = PokemonAcquired.From(specimen);
    await _messagingManager.PublishAsync(acquired, cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
