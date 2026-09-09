using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Evolutions.Models;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
using PokeGame.Core.Permissions;

namespace PokeGame.Core.Evolutions.Commands;

internal record CreateOrReplaceEvolutionCommand(CreateOrReplaceEvolutionPayload Payload, Guid? Id) : ICommand<CreateOrReplaceEvolutionResult>;

internal class CreateOrReplaceEvolutionCommandHandler : ICommandHandler<CreateOrReplaceEvolutionCommand, CreateOrReplaceEvolutionResult>
{
  private readonly IContext _context;
  private readonly IEvolutionQuerier _evolutionQuerier;
  private readonly IEvolutionRepository _evolutionRepository;
  private readonly IItemRepository _itemRepository;
  private readonly IPermissionService _permissionService;

  public CreateOrReplaceEvolutionCommandHandler(
    IContext context,
    IEvolutionQuerier evolutionQuerier,
    IEvolutionRepository evolutionRepository,
    IItemRepository itemRepository,
    IPermissionService permissionService)
  {
    _context = context;
    _evolutionQuerier = evolutionQuerier;
    _evolutionRepository = evolutionRepository;
    _itemRepository = itemRepository;
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

    ActorId? actorId = _context.ActorId;

    bool created = false;
    if (evolution is null)
    {
      await _permissionService.CheckAsync(Actions.CreateEvolution, cancellationToken);

      Form source = null!; // TODO(fpion): implement
      Form target = null!; // TODO(fpion): implement
      Item? item = null; // TODO(fpion): implement

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

    // TODO(fpion): conditions

    await _evolutionRepository.SaveAsync(evolution, cancellationToken);

    EvolutionDto dto = await _evolutionQuerier.ReadAsync(evolution, cancellationToken);
    return new CreateOrReplaceEvolutionResult(dto, created);
  }
}
