using Logitar.CQRS;
using PokeGame.Core.Evolutions.Models;
using PokeGame.Core.Items;
using PokeGame.Core.Moves;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Regions;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Evolutions.Commands;

internal record UpdateEvolutionCommand(Guid Id, UpdateEvolutionPayload Payload) : ICommand<EvolutionModel?>;

internal class UpdateEvolutionCommandHandler : ICommandHandler<UpdateEvolutionCommand, EvolutionModel?>
{
  private readonly IContext _context;
  private readonly IEvolutionQuerier _evolutionQuerier;
  private readonly IEvolutionRepository _evolutionRepository;
  private readonly IItemManager _itemManager;
  private readonly IMoveManager _moveManager;
  private readonly IPermissionService _permissionService;
  private readonly IStorageService _storageService;

  public UpdateEvolutionCommandHandler(
    IContext context,
    IEvolutionQuerier evolutionQuerier,
    IEvolutionRepository evolutionRepository,
    IItemManager itemManager,
    IMoveManager moveManager,
    IPermissionService permissionService,
    IStorageService storageService)
  {
    _context = context;
    _evolutionQuerier = evolutionQuerier;
    _evolutionRepository = evolutionRepository;
    _itemManager = itemManager;
    _moveManager = moveManager;
    _permissionService = permissionService;
    _storageService = storageService;
  }

  public async Task<EvolutionModel?> HandleAsync(UpdateEvolutionCommand command, CancellationToken cancellationToken)
  {
    UpdateEvolutionPayload payload = command.Payload;
    payload.Validate();

    EvolutionId evolutionId = new(_context.WorldId, command.Id);
    Evolution? evolution = await _evolutionRepository.LoadAsync(evolutionId, cancellationToken);
    if (evolution is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, evolution, cancellationToken);

    if (payload.Level is not null)
    {
      evolution.Level = payload.Level.Value.HasValue ? new Level(payload.Level.Value.Value) : null;
    }
    if (payload.Friendship.HasValue)
    {
      evolution.Friendship = payload.Friendship.Value;
    }
    if (payload.Gender is not null)
    {
      evolution.Gender = payload.Gender.Value;
    }
    if (payload.HeldItem is not null)
    {
      Item? heldItem = null;
      if (!string.IsNullOrWhiteSpace(payload.HeldItem.Value))
      {
        heldItem = await _itemManager.FindAsync(payload.HeldItem.Value, nameof(payload.HeldItem), cancellationToken);
      }
      evolution.HeldItemId = heldItem?.Id;
    }
    if (payload.KnownMove is not null)
    {
      Move? knownMove = null;
      if (!string.IsNullOrWhiteSpace(payload.KnownMove.Value))
      {
        knownMove = await _moveManager.FindAsync(payload.KnownMove.Value, nameof(payload.KnownMove), cancellationToken);
      }
      evolution.KnownMoveId = knownMove?.Id;
    }
    if (payload.Location is not null)
    {
      evolution.Location = Location.TryCreate(payload.Location.Value);
    }
    if (payload.TimeOfDay is not null)
    {
      evolution.TimeOfDay = payload.TimeOfDay.Value;
    }

    evolution.Update(_context.UserId);

    await _storageService.ExecuteWithQuotaAsync(
      evolution,
      async () => await _evolutionRepository.SaveAsync(evolution, cancellationToken),
      cancellationToken);

    return await _evolutionQuerier.ReadAsync(evolution, cancellationToken);
  }
}
