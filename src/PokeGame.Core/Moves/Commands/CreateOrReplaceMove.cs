using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Moves.Models;
using PokeGame.Core.Permissions;

namespace PokeGame.Core.Moves.Commands;

internal record CreateOrReplaceMoveCommand(CreateOrReplaceMovePayload Payload, Guid? Id) : ICommand<CreateOrReplaceMoveResult>;

internal class CreateOrReplaceMoveCommandHandler : ICommandHandler<CreateOrReplaceMoveCommand, CreateOrReplaceMoveResult>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IMoveManager _moveManager;
  private readonly IMoveQuerier _moveQuerier;
  private readonly IMoveRepository _moveRepository;

  public CreateOrReplaceMoveCommandHandler(
    IContext context,
    IPermissionService permissionService,
    IMoveManager moveManager,
    IMoveQuerier moveQuerier,
    IMoveRepository moveRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _moveManager = moveManager;
    _moveQuerier = moveQuerier;
    _moveRepository = moveRepository;
  }

  public async Task<CreateOrReplaceMoveResult> HandleAsync(CreateOrReplaceMoveCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceMovePayload payload = command.Payload;
    payload.Validate();

    MoveId? moveId = null;
    Move? move = null;
    if (command.Id.HasValue)
    {
      moveId = new(_context.WorldId, command.Id.Value);
      move = await _moveRepository.LoadAsync(moveId.Value, cancellationToken);
    }

    ActorId? actorId = _context.ActorId;
    Key key = new(payload.Key);

    bool created = false;
    if (move is null)
    {
      await _permissionService.CheckAsync(Actions.CreateMove, cancellationToken);

      move = new Move(moveId ?? MoveId.NewId(_context.WorldId), payload.Type, payload.Category, key, actorId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, move, cancellationToken);

      if (payload.Type != move.Type)
      {
        throw new ImmutablePropertyException<PokemonType>(move, move.Type, payload.Type, nameof(payload.Type));
      }
      if (payload.Category != move.Category)
      {
        throw new ImmutablePropertyException<MoveCategory>(move, move.Category, payload.Category, nameof(payload.Category));
      }

      move.SetKey(key, actorId);
    }

    move.Update(
      Name.TryCreate(payload.Name),
      Summary.TryCreate(payload.Summary),
      Content.TryCreate(payload.Content),
      Accuracy.TryCreate(payload.Accuracy),
      Power.TryCreate(payload.Power),
      PowerPoints.TryCreate(payload.PowerPoints),
      actorId);

    await _moveManager.EnsureUnicityAsync(move, cancellationToken);
    await _moveRepository.SaveAsync(move, cancellationToken);

    MoveDto dto = await _moveQuerier.ReadAsync(move, cancellationToken);
    return new CreateOrReplaceMoveResult(dto, created);
  }
}
