using Logitar.CQRS;
using PokeGame.Core.Moves.Events;
using PokeGame.Core.Moves.Models;
using PokeGame.Core.Permissions;

namespace PokeGame.Core.Moves.Commands;

internal record UpdateMoveCommand(Guid Id, UpdateMovePayload Payload) : ICommand<MoveModel?>;

internal class UpdateMoveCommandHandler : ICommandHandler<UpdateMoveCommand, MoveModel?>
{
  private readonly IMoveRepository _moveRepository;
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;

  public UpdateMoveCommandHandler(IMoveRepository moveRepository, IContext context, IPermissionService permissionService)
  {
    _moveRepository = moveRepository;
    _context = context;
    _permissionService = permissionService;
  }

  public async Task<MoveModel?> HandleAsync(UpdateMoveCommand command, CancellationToken cancellationToken)
  {
    UpdateMovePayload payload = command.Payload;
    payload.Validate();

    Move? move = await _moveRepository.LoadAsync(command.Id, cancellationToken);
    if (move is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, move, cancellationToken);

    MoveUpdated record = move.Update(
      string.IsNullOrWhiteSpace(payload.Key) ? move.Key : payload.Key,
      payload.Name is null ? move.Name : payload.Name.Value,
      payload.Description is null ? move.Description : payload.Description.Value,
      payload.Accuracy is null ? move.Accuracy : payload.Accuracy.Value,
      payload.Power is null ? move.Power : payload.Power.Value,
      payload.PowerPoints ?? move.PowerPoints,
      _context.UserId);
    _moveRepository.Update(move, record);

    await _moveRepository.EnsureUnicityAsync(move, cancellationToken);

    await _context.SaveChangesAsync(cancellationToken);

    return await _moveRepository.ReadAsync(move, cancellationToken);
  }
}
