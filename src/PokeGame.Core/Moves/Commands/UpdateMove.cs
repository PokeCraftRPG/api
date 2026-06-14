using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Moves.Models;
using PokeGame.Core.Permissions;

namespace PokeGame.Core.Moves.Commands;

internal record UpdateMoveCommand(Guid Id, UpdateMovePayload Payload) : ICommand<MoveModel?>;

internal class UpdateMoveCommandHandler : ICommandHandler<UpdateMoveCommand, MoveModel?>
{
  private readonly IContext _context;
  private readonly IMoveManager _moveManager;
  private readonly IMoveQuerier _moveQuerier;
  private readonly IMoveRepository _moveRepository;
  private readonly IPermissionService _permissionService;

  public UpdateMoveCommandHandler(
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

  public async Task<MoveModel?> HandleAsync(UpdateMoveCommand command, CancellationToken cancellationToken)
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
      move.SetKey(new Slug(payload.Key), actorId);
    }
    if (payload.Name is not null)
    {
      move.Rename(Name.TryCreate(payload.Name.Value), actorId);
    }
    if (payload.Description is not null)
    {
      move.Describe(Description.TryCreate(payload.Description.Value), actorId);
    }

    await _moveManager.EnsureUnicityAsync(move, cancellationToken);
    await _moveRepository.SaveAsync(move, cancellationToken);

    return await _moveQuerier.FindAsync(move, cancellationToken);
  }
}
