using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Moves.Models;
using PokeGame.Core.Permissions;

namespace PokeGame.Core.Moves.Commands;

internal record UpdateMoveCommand(Guid Id, UpdateMovePayload Payload) : ICommand<MoveDto?>;

internal class UpdateMoveCommandHandler : ICommandHandler<UpdateMoveCommand, MoveDto?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IMoveManager _moveManager;
  private readonly IMoveQuerier _moveQuerier;
  private readonly IMoveRepository _moveRepository;

  public UpdateMoveCommandHandler(
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

  public async Task<MoveDto?> HandleAsync(UpdateMoveCommand command, CancellationToken cancellationToken)
  {
    UpdateMovePayload payload = command.Payload;
    payload.Validate();

    MoveId moveId = new(_context.WorldId, command.Id);
    Move? move = await _moveRepository.LoadAsync(moveId, cancellationToken);
    if (move is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, move, cancellationToken);

    ActorId? actorId = _context.ActorId;

    if (!string.IsNullOrWhiteSpace(payload.Key))
    {
      move.SetKey(new Key(payload.Key), actorId);
    }

    if (payload.Name is not null || payload.Summary is not null || payload.Content is not null)
    {
      move.Update(
        payload.Name is null ? move.Name : Name.TryCreate(payload.Name.Value),
        payload.Summary is null ? move.Summary : Summary.TryCreate(payload.Summary.Value),
        payload.Content is null ? move.Content : Content.TryCreate(payload.Content.Value),
        actorId);
    }

    if (payload.Accuracy is not null || payload.Power is not null || payload.PowerPoints is not null)
    {
      move.SetMechanics(
        payload.Accuracy is null ? move.Accuracy : Accuracy.TryCreate(payload.Accuracy.Value),
        payload.Power is null ? move.Power : Power.TryCreate(payload.Power.Value),
        payload.PowerPoints is null ? move.PowerPoints : PowerPoints.TryCreate(payload.PowerPoints.Value),
        actorId);
    }

    await _moveManager.EnsureUnicityAsync(move, cancellationToken);
    await _moveRepository.SaveAsync(move, cancellationToken);

    return await _moveQuerier.ReadAsync(move, cancellationToken);
  }
}
