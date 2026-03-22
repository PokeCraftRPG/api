using Logitar.CQRS;
using PokeGame.Core.Moves.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Moves.Commands;

internal record CreateOrReplaceMoveCommand(CreateOrReplaceMovePayload Payload, Guid? Id) : ICommand<CreateOrReplaceMoveResult>;

internal class CreateOrReplaceMoveCommandHandler : ICommandHandler<CreateOrReplaceMoveCommand, CreateOrReplaceMoveResult>
{
  private readonly IContext _context;
  private readonly IMoveQuerier _moveQuerier;
  private readonly IMoveRepository _moveRepository;
  private readonly IPermissionService _permissionService;
  private readonly IStorageService _storageService;

  public CreateOrReplaceMoveCommandHandler(
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

  public async Task<CreateOrReplaceMoveResult> HandleAsync(CreateOrReplaceMoveCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceMovePayload payload = command.Payload;
    payload.Validate();

    UserId userId = _context.UserId;
    WorldId worldId = _context.WorldId;

    MoveId moveId = MoveId.NewId(worldId);
    Move? move = null;
    if (command.Id.HasValue)
    {
      moveId = new(worldId, command.Id.Value);
      move = await _moveRepository.LoadAsync(moveId, cancellationToken);
    }

    Name name = new(payload.Name);
    PowerPoints powerPoints = new(payload.PowerPoints);

    bool created = false;
    if (move is null)
    {
      await _permissionService.CheckAsync(Actions.CreateMove, cancellationToken);

      move = new(payload.Type, payload.Category, name, powerPoints, userId, moveId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, move, cancellationToken);

      if (payload.Type != move.Type)
      {
        // TODO(fpion): implement
      }
      if (payload.Category != move.Category)
      {
        // TODO(fpion): implement
      }

      move.Name = name;

      move.PowerPoints = powerPoints;
    }

    move.Description = Description.TryCreate(payload.Description);

    move.Accuracy = payload.Accuracy.HasValue ? new Accuracy(payload.Accuracy.Value) : null;
    move.Power = payload.Power.HasValue ? new Power(payload.Power.Value) : null;

    move.Url = Url.TryCreate(payload.Url);
    move.Notes = Notes.TryCreate(payload.Notes);

    move.Update(userId);

    await _storageService.ExecuteWithQuotaAsync(
      move,
      async () => await _moveRepository.SaveAsync(move, cancellationToken),
      cancellationToken);

    MoveModel model = await _moveQuerier.ReadAsync(move, cancellationToken);
    return new CreateOrReplaceMoveResult(model, created);
  }
}
