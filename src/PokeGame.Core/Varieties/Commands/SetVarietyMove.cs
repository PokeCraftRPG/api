using Logitar.CQRS;
using PokeGame.Core.Moves;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Varieties.Commands;

internal record SetVarietyMoveCommand(Guid VarietyId, Guid? Id, SetVarietyMovePayload Payload) : ICommand<VarietyDto>;

internal class SetVarietyMoveCommandHandler : ICommandHandler<SetVarietyMoveCommand, VarietyDto>
{
  private readonly IContext _context;
  private readonly IMoveRepository _moveRepository;
  private readonly IPermissionService _permissionService;
  private readonly IVarietyQuerier _varietyQuerier;
  private readonly IVarietyRepository _varietyRepository;

  public SetVarietyMoveCommandHandler(
    IContext context,
    IMoveRepository moveRepository,
    IPermissionService permissionService,
    IVarietyQuerier varietyQuerier,
    IVarietyRepository varietyRepository)
  {
    _context = context;
    _moveRepository = moveRepository;
    _permissionService = permissionService;
    _varietyQuerier = varietyQuerier;
    _varietyRepository = varietyRepository;
  }

  public async Task<VarietyDto> HandleAsync(SetVarietyMoveCommand command, CancellationToken cancellationToken)
  {
    SetVarietyMovePayload payload = command.Payload;
    payload.Validate();

    VarietyId varietyId = new(_context.WorldId, command.VarietyId);
    Variety variety = await _varietyRepository.LoadAsync(varietyId, cancellationToken)
      ?? throw new EntityNotFoundException(varietyId, nameof(command.VarietyId));
    await _permissionService.CheckAsync(Actions.Update, variety, cancellationToken);

    if (command.Id.HasValue)
    {
      VarietyMove? varietyMove = variety.TryGetMove(command.Id.Value);
      if (varietyMove is null)
      {
        await AddMoveAsync(variety, payload, command.Id.Value, cancellationToken);
      }
      else if (payload.MoveId != varietyMove.MoveId.EntityId)
      {
        throw new ImmutablePropertyException<Guid>(variety, varietyMove.MoveId.EntityId, payload.MoveId, nameof(payload.MoveId));
      }
      else
      {
        varietyMove = new VarietyMove(varietyMove.MoveId, payload.LearningMethod, Level.TryCreate(payload.Level));
        variety.SetMove(command.Id.Value, varietyMove, _context.ActorId);
      }
    }
    else
    {
      await AddMoveAsync(variety, payload, id: null, cancellationToken);
    }

    await _varietyRepository.SaveAsync(variety, cancellationToken);

    return await _varietyQuerier.ReadAsync(variety, cancellationToken);
  }

  private async Task AddMoveAsync(Variety variety, SetVarietyMovePayload payload, Guid? id, CancellationToken cancellationToken)
  {
    MoveId moveId = new(variety.WorldId, payload.MoveId);
    Move move = await _moveRepository.LoadAsync(moveId, cancellationToken) ?? throw new EntityNotFoundException(moveId, nameof(payload.MoveId));

    VarietyMove varietyMove = new(move.Id, payload.LearningMethod, Level.TryCreate(payload.Level));
    if (id.HasValue)
    {
      variety.SetMove(id.Value, varietyMove, _context.ActorId);
    }
    else
    {
      variety.AddMove(varietyMove, _context.ActorId);
    }
  }
}
