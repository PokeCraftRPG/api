using Logitar.CQRS;
using PokeGame.Core.Evolutions.Models;
using PokeGame.Core.Items;
using PokeGame.Core.Moves;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Regions;

namespace PokeGame.Core.Evolutions.Commands;

internal record UpdateEvolutionCommand(Guid Id, UpdateEvolutionPayload Payload) : ICommand<EvolutionDto?>;

internal class UpdateEvolutionCommandHandler : ICommandHandler<UpdateEvolutionCommand, EvolutionDto?>
{
  private readonly IContext _context;
  private readonly IEvolutionQuerier _evolutionQuerier;
  private readonly IEvolutionRepository _evolutionRepository;
  private readonly IItemRepository _itemRepository;
  private readonly IMoveRepository _moveRepository;
  private readonly IPermissionService _permissionService;

  public UpdateEvolutionCommandHandler(
    IContext context,
    IEvolutionQuerier evolutionQuerier,
    IEvolutionRepository evolutionRepository,
    IItemRepository itemRepository,
    IMoveRepository moveRepository,
    IPermissionService permissionService)
  {
    _context = context;
    _evolutionQuerier = evolutionQuerier;
    _evolutionRepository = evolutionRepository;
    _itemRepository = itemRepository;
    _moveRepository = moveRepository;
    _permissionService = permissionService;
  }

  public async Task<EvolutionDto?> HandleAsync(UpdateEvolutionCommand command, CancellationToken cancellationToken)
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

    if (payload.Level is not null || payload.Friendship is not null || payload.Gender is not null || payload.ItemId is not null
      || payload.MoveId is not null || payload.Location is not null || payload.TimeOfDay is not null)
    {
      Item? item = await LoadItemAsync(evolution, payload.ItemId, nameof(payload.ItemId), cancellationToken);
      Move? move = await LoadMoveAsync(evolution, payload.MoveId, nameof(payload.MoveId), cancellationToken);
      evolution.SetConditions(
        payload.Level is null ? evolution.Level : Level.TryCreate(payload.Level.Value),
        payload.Friendship ?? evolution.Friendship,
        payload.Gender is null ? evolution.Gender : payload.Gender.Value,
        item,
        move,
        payload.Location is null ? evolution.Location : Location.TryCreate(payload.Location.Value),
        payload.TimeOfDay is null ? evolution.TimeOfDay : payload.TimeOfDay.Value,
        _context.ActorId);
    }

    await _evolutionRepository.SaveAsync(evolution, cancellationToken);

    return await _evolutionQuerier.ReadAsync(evolution, cancellationToken);
  }

  private async Task<Item?> LoadItemAsync(Evolution evolution, Optional<Guid?>? itemId, string propertyName, CancellationToken cancellationToken)
  {
    if (itemId is null)
    {
      if (evolution.ItemId.HasValue)
      {
        return await _itemRepository.LoadAsync(evolution.ItemId.Value, cancellationToken)
          ?? throw new InvalidOperationException($"The item 'Id={evolution.ItemId}' was not loaded.");
      }
      return null;
    }
    else if (itemId.Value.HasValue)
    {
      ItemId streamId = new(evolution.WorldId, itemId.Value.Value);
      return await _itemRepository.LoadAsync(streamId, cancellationToken) ?? throw new EntityNotFoundException(streamId, propertyName);
    }
    return null;
  }

  private async Task<Move?> LoadMoveAsync(Evolution evolution, Optional<Guid?>? moveId, string propertyName, CancellationToken cancellationToken)
  {
    if (moveId is null)
    {
      if (evolution.MoveId.HasValue)
      {
        return await _moveRepository.LoadAsync(evolution.MoveId.Value, cancellationToken)
          ?? throw new InvalidOperationException($"The move 'Id={evolution.MoveId}' was not loaded.");
      }
      return null;
    }
    else if (moveId.Value.HasValue)
    {
      MoveId streamId = new(evolution.WorldId, moveId.Value.Value);
      return await _moveRepository.LoadAsync(streamId, cancellationToken) ?? throw new EntityNotFoundException(streamId, propertyName);
    }
    return null;
  }
}
