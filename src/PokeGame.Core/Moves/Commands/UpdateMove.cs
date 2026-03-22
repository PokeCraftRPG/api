using Logitar.CQRS;
using PokeGame.Core.Moves.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Moves.Commands;

internal record UpdateMoveCommand(Guid Id, UpdateMovePayload Payload) : ICommand<MoveModel?>;

internal class UpdateMoveCommandHandler : ICommandHandler<UpdateMoveCommand, MoveModel?>
{
  private readonly IContext _context;
  private readonly IMoveQuerier _moveQuerier;
  private readonly IMoveRepository _moveRepository;
  private readonly IPermissionService _permissionService;
  private readonly IStorageService _storageService;

  public UpdateMoveCommandHandler(
    IContext context,
    IMoveQuerier moveQuerier,
    IMoveRepository moveRepository,
    IPermissionService permissionService,
    IStorageService storageService)
  {
    _context = context;
    _moveQuerier = moveQuerier;
    _moveRepository = moveRepository;
    _permissionService = permissionService;
    _storageService = storageService;
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

    if (!string.IsNullOrWhiteSpace(payload.Name))
    {
      move.Name = new Name(payload.Name);
    }
    if (payload.Description is not null)
    {
      move.Description = Description.TryCreate(payload.Description.Value);
    }

    if (payload.Url is not null)
    {
      move.Url = Url.TryCreate(payload.Url.Value);
    }
    if (payload.Notes is not null)
    {
      move.Notes = Notes.TryCreate(payload.Notes.Value);
    }

    move.Update(_context.UserId);

    await _storageService.ExecuteWithQuotaAsync(
      move,
      async () => await _moveRepository.SaveAsync(move, cancellationToken),
      cancellationToken);

    return await _moveQuerier.ReadAsync(move, cancellationToken);
  }
}
