using Logitar.CQRS;
using PokeGame.Core.Moves.Models;
using PokeGame.Core.Permissions;

namespace PokeGame.Core.Moves.Commands;

internal record DeleteMoveCommand(Guid Id) : ICommand<MoveModel?>;

internal class DeleteMoveCommandHandler : ICommandHandler<DeleteMoveCommand, MoveModel?>
{
  private readonly IMoveRepository _moveRepository;
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;

  public DeleteMoveCommandHandler(IMoveRepository moveRepository, IContext context, IPermissionService permissionService)
  {
    _moveRepository = moveRepository;
    _context = context;
    _permissionService = permissionService;
  }

  public async Task<MoveModel?> HandleAsync(DeleteMoveCommand command, CancellationToken cancellationToken)
  {
    Move? move = await _moveRepository.LoadAsync(command.Id, cancellationToken);
    if (move is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Delete, move, cancellationToken);

    MoveModel model = await _moveRepository.ReadAsync(move, cancellationToken);

    _moveRepository.Remove(move);

    await _context.SaveChangesAsync(cancellationToken);

    return model;
  }
}
