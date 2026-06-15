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
    IMoveManager moveManager,
    IMoveQuerier moveQuerier,
    IMoveRepository moveRepository,
    IPermissionService permissionService)
  {
    _context = context;
    _moveManager = moveManager;
    _moveQuerier = moveQuerier;
    _moveRepository = moveRepository;
    _permissionService = permissionService;
  }

  public async Task<CreateOrReplaceMoveResult> HandleAsync(CreateOrReplaceMoveCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceMovePayload payload = command.Payload;
    payload.Validate();

    MoveId moveId = MoveId.NewId(_context.WorldId);
    Move? move = null;
    if (command.Id.HasValue)
    {
      moveId = new MoveId(moveId.WorldId, command.Id.Value);
      move = await _moveRepository.LoadAsync(moveId, cancellationToken);
    }

    Slug key = new(payload.Key);
    ActorId? actorId = _context.ActorId;

    bool created = false;
    if (move is null)
    {
      await _permissionService.CheckAsync(Actions.CreateMove, cancellationToken);

      move = new Move(moveId, payload.Type, payload.Category, key, actorId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, move, cancellationToken);

      if (payload.Type != move.Type)
      {
        throw new NotImplementedException(); // TODO(fpion): implement
      }
      if (payload.Category != move.Category)
      {
        throw new NotImplementedException(); // TODO(fpion): implement
      }

      move.SetKey(key, actorId);
    }

    move.Rename(Name.TryCreate(payload.Name), actorId);
    move.Describe(Description.TryCreate(payload.Description), actorId);
    move.SetGameData(Accuracy.TryCreate(payload.Accuracy), Power.TryCreate(payload.Power), new PowerPoints(payload.PowerPoints), actorId);

    await _moveManager.EnsureUnicityAsync(move, cancellationToken);
    await _moveRepository.SaveAsync(move, cancellationToken);

    MoveModel model = await _moveQuerier.FindAsync(move, cancellationToken);
    return new CreateOrReplaceMoveResult(model, created);
  }
}
