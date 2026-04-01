using Logitar.CQRS;
using PokeGame.Core.Evolutions.Models;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
using PokeGame.Core.Moves;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Regions;
using PokeGame.Core.Storages;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Evolutions.Commands;

internal record CreateOrReplaceEvolutionCommand(CreateOrReplaceEvolutionPayload Payload, Guid? Id) : ICommand<CreateOrReplaceEvolutionResult>;

internal class CreateOrReplaceEvolutionCommandHandler : ICommandHandler<CreateOrReplaceEvolutionCommand, CreateOrReplaceEvolutionResult>
{
  private readonly IContext _context;
  private readonly IEvolutionQuerier _evolutionQuerier;
  private readonly IEvolutionRepository _evolutionRepository;
  private readonly IFormManager _formManager;
  private readonly IItemManager _itemManager;
  private readonly IMoveManager _moveManager;
  private readonly IPermissionService _permissionService;
  private readonly IStorageService _storageService;

  public CreateOrReplaceEvolutionCommandHandler(
    IContext context,
    IEvolutionQuerier evolutionQuerier,
    IEvolutionRepository evolutionRepository,
    IFormManager formManager,
    IItemManager itemManager,
    IMoveManager moveManager,
    IPermissionService permissionService,
    IStorageService storageService)
  {
    _context = context;
    _evolutionQuerier = evolutionQuerier;
    _evolutionRepository = evolutionRepository;
    _formManager = formManager;
    _itemManager = itemManager;
    _moveManager = moveManager;
    _permissionService = permissionService;
    _storageService = storageService;
  }

  public async Task<CreateOrReplaceEvolutionResult> HandleAsync(CreateOrReplaceEvolutionCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceEvolutionPayload payload = command.Payload;
    payload.Validate();

    UserId userId = _context.UserId;
    WorldId worldId = _context.WorldId;

    EvolutionId evolutionId = EvolutionId.NewId(worldId);
    Evolution? evolution = null;
    if (command.Id.HasValue)
    {
      evolutionId = new(worldId, command.Id.Value);
      evolution = await _evolutionRepository.LoadAsync(evolutionId, cancellationToken);
    }

    Form source = await _formManager.FindAsync(payload.Source, nameof(payload.Source), cancellationToken);
    Form target = await _formManager.FindAsync(payload.Target, nameof(payload.Target), cancellationToken);
    Item? item = string.IsNullOrWhiteSpace(payload.Item) ? null : await _itemManager.FindAsync(payload.Item, nameof(payload.Item), cancellationToken);

    bool created = false;
    if (evolution is null)
    {
      await _permissionService.CheckAsync(Actions.CreateEvolution, cancellationToken);

      evolution = new(source, target, payload.Trigger, item, userId, evolutionId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, evolution, cancellationToken);

      if (source.Id != evolution.SourceId)
      {
        throw new ImmutablePropertyException<Guid>(evolution, evolution.SourceId.EntityId, source.EntityId, nameof(payload.Source));
      }
      if (target.Id != evolution.TargetId)
      {
        throw new ImmutablePropertyException<Guid>(evolution, evolution.TargetId.EntityId, target.EntityId, nameof(payload.Target));
      }
      if (payload.Trigger != evolution.Trigger)
      {
        throw new ImmutablePropertyException<EvolutionTrigger>(evolution, evolution.Trigger, payload.Trigger, nameof(payload.Trigger));
      }
      if (item?.Id != evolution.ItemId)
      {
        throw new ImmutablePropertyException<Guid?>(evolution, evolution.ItemId?.EntityId, item?.EntityId, nameof(payload.Item));
      }
    }

    evolution.Level = payload.Level.HasValue ? new Level(payload.Level.Value) : null;
    evolution.Friendship = payload.Friendship;
    evolution.Gender = payload.Gender;

    Item? heldItem = string.IsNullOrWhiteSpace(payload.HeldItem) ? null : await _itemManager.FindAsync(payload.HeldItem, nameof(payload.HeldItem), cancellationToken);
    evolution.HeldItemId = heldItem?.Id;

    Move? knownMove = string.IsNullOrWhiteSpace(payload.KnownMove) ? null : await _moveManager.FindAsync(payload.KnownMove, nameof(payload.KnownMove), cancellationToken);
    evolution.KnownMoveId = knownMove?.Id;

    evolution.Location = Location.TryCreate(payload.Location);
    evolution.TimeOfDay = payload.TimeOfDay;

    evolution.Update(userId);

    await _storageService.ExecuteWithQuotaAsync(
      evolution,
      async () => await _evolutionRepository.SaveAsync(evolution, cancellationToken),
      cancellationToken);

    EvolutionModel model = await _evolutionQuerier.ReadAsync(evolution, cancellationToken);
    return new CreateOrReplaceEvolutionResult(model, created);
  }
}
