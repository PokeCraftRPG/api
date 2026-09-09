using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Evolutions.Models;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
using PokeGame.Core.Moves;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Regions;

namespace PokeGame.Core.Evolutions.Commands;

internal record CreateOrReplaceEvolutionCommand(CreateOrReplaceEvolutionPayload Payload, Guid? Id) : ICommand<CreateOrReplaceEvolutionResult>;

internal class CreateOrReplaceEvolutionCommandHandler : ICommandHandler<CreateOrReplaceEvolutionCommand, CreateOrReplaceEvolutionResult>
{
  private readonly IContext _context;
  private readonly IEvolutionQuerier _evolutionQuerier;
  private readonly IEvolutionRepository _evolutionRepository;
  private readonly IFormRepository _formRepository;
  private readonly IItemRepository _itemRepository;
  private readonly IMoveRepository _moveRepository;
  private readonly IPermissionService _permissionService;

  public CreateOrReplaceEvolutionCommandHandler(
    IContext context,
    IEvolutionQuerier evolutionQuerier,
    IEvolutionRepository evolutionRepository,
    IFormRepository formRepository,
    IItemRepository itemRepository,
    IMoveRepository moveRepository,
    IPermissionService permissionService)
  {
    _context = context;
    _evolutionQuerier = evolutionQuerier;
    _evolutionRepository = evolutionRepository;
    _formRepository = formRepository;
    _itemRepository = itemRepository;
    _moveRepository = moveRepository;
    _permissionService = permissionService;
  }

  public async Task<CreateOrReplaceEvolutionResult> HandleAsync(CreateOrReplaceEvolutionCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceEvolutionPayload payload = command.Payload;
    payload.Validate();

    EvolutionId evolutionId = EvolutionId.NewId(_context.WorldId);
    Evolution? evolution = null;
    if (command.Id.HasValue)
    {
      evolutionId = new EvolutionId(evolutionId.WorldId, command.Id.Value);
      evolution = await _evolutionRepository.LoadAsync(evolutionId, cancellationToken);
    }

    Item? item = null;
    if (payload.ItemId.HasValue)
    {
      ItemId itemId = new(evolutionId.WorldId, payload.ItemId.Value);
      item = await _itemRepository.LoadAsync(itemId, cancellationToken) ?? throw new EntityNotFoundException(itemId, nameof(payload.ItemId));
    }

    Move? move = null;
    if (payload.MoveId.HasValue)
    {
      MoveId moveId = new(evolutionId.WorldId, payload.MoveId.Value);
      move = await _moveRepository.LoadAsync(moveId, cancellationToken) ?? throw new EntityNotFoundException(moveId, nameof(payload.MoveId));
    }

    ActorId? actorId = _context.ActorId;

    bool created = false;
    if (evolution is null)
    {
      await _permissionService.CheckAsync(Actions.CreateEvolution, cancellationToken);

      FormId sourceId = new(evolutionId.WorldId, payload.SourceId);
      Form source = await _formRepository.LoadAsync(sourceId, cancellationToken) ?? throw new EntityNotFoundException(sourceId, nameof(payload.SourceId));

      FormId targetId = new(evolutionId.WorldId, payload.TargetId);
      Form target = await _formRepository.LoadAsync(targetId, cancellationToken) ?? throw new EntityNotFoundException(targetId, nameof(payload.TargetId));

      evolution = new Evolution(evolutionId, source, target, payload.Trigger, item, actorId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, evolution, cancellationToken);

      if (payload.SourceId != evolution.SourceId.EntityId)
      {
        throw new ImmutablePropertyException<Guid>(evolution, evolution.SourceId.EntityId, payload.SourceId, nameof(payload.SourceId));
      }
      if (payload.TargetId != evolution.TargetId.EntityId)
      {
        throw new ImmutablePropertyException<Guid>(evolution, evolution.TargetId.EntityId, payload.TargetId, nameof(payload.TargetId));
      }
      if (payload.Trigger != evolution.Trigger)
      {
        throw new ImmutablePropertyException<EvolutionTrigger>(evolution, evolution.Trigger, payload.Trigger, nameof(payload.Trigger));
      }
    }

    evolution.SetConditions(
      Level.TryCreate(payload.Level),
      payload.Friendship,
      payload.Gender,
      item,
      move,
      Location.TryCreate(payload.Location),
      payload.TimeOfDay,
      actorId);

    await _evolutionRepository.SaveAsync(evolution, cancellationToken);

    EvolutionDto dto = await _evolutionQuerier.ReadAsync(evolution, cancellationToken);
    return new CreateOrReplaceEvolutionResult(dto, created);
  }
}
