using Logitar.CQRS;
using PokeGame.Core.Moves.Events;
using PokeGame.Core.Moves.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Moves.Commands;

internal record CreateOrReplaceMoveCommand(CreateOrReplaceMovePayload Payload, Guid? Id) : ICommand<CreateOrReplaceMoveResult>;

internal class CreateOrReplaceMoveCommandHandler : ICommandHandler<CreateOrReplaceMoveCommand, CreateOrReplaceMoveResult>
{
  private readonly IContext _context;
  private readonly IMoveRepository _moveRepository;
  private readonly IPermissionService _permissionService;
  private readonly IWorldRepository _worldRepository;

  public CreateOrReplaceMoveCommandHandler(
    IContext context,
    IMoveRepository moveRepository,
    IPermissionService permissionService,
    IWorldRepository worldRepository)
  {
    _context = context;
    _moveRepository = moveRepository;
    _permissionService = permissionService;
    _worldRepository = worldRepository;
  }

  public async Task<CreateOrReplaceMoveResult> HandleAsync(CreateOrReplaceMoveCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceMovePayload payload = command.Payload;
    payload.Validate();

    Move? move = null;
    if (command.Id.HasValue)
    {
      move = await _moveRepository.LoadAsync(command.Id.Value, cancellationToken);
    }

    bool created = false;
    if (move is null)
    {
      World world = await _worldRepository.LoadAsync(_context.WorldId, cancellationToken)
        ?? throw new InvalidOperationException($"The world 'Id={_context.WorldId}' was not loaded.");
      await _permissionService.CheckAsync(Actions.CreateMove, world, cancellationToken);

      move = new Move(
        world,
        payload.Type,
        payload.Category,
        payload.Key,
        payload.PowerPoints,
        _context.UserId,
        command.Id,
        payload.Name,
        payload.Description,
        payload.Accuracy,
        payload.Power);
      _moveRepository.Add(move);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, move, cancellationToken);

      if (payload.Type != move.Type)
      {
        throw new ImmutablePropertyException<PokemonType>(move, move.Type, payload.Type, nameof(Move.Type));
      }
      if (payload.Category != move.Category)
      {
        throw new ImmutablePropertyException<MoveCategory>(move, move.Category, payload.Category, nameof(Move.Category));
      }

      MoveUpdated record = move.Update(
        payload.Key,
        payload.Name,
        payload.Description,
        payload.Accuracy,
        payload.Power,
        payload.PowerPoints,
        _context.UserId);
      _moveRepository.Update(move, record);
    }

    await _moveRepository.EnsureUnicityAsync(move, cancellationToken);

    await _context.SaveChangesAsync(cancellationToken);

    MoveModel model = await _moveRepository.ReadAsync(move, cancellationToken);
    return new CreateOrReplaceMoveResult(model, created);
  }
}
