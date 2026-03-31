using PokeGame.Core.Worlds;

namespace PokeGame.Core.Moves;

public interface IMoveManager
{
  Task<Move> FindAsync(string move, string propertyName, CancellationToken cancellationToken = default);
}

internal class MoveManager : IMoveManager
{
  private readonly IContext _context;
  private readonly IMoveQuerier _moveQuerier;
  private readonly IMoveRepository _moveRepository;

  public MoveManager(IContext context, IMoveQuerier moveQuerier, IMoveRepository moveRepository)
  {
    _context = context;
    _moveQuerier = moveQuerier;
    _moveRepository = moveRepository;
  }

  public async Task<Move> FindAsync(string idOrKey, string propertyName, CancellationToken cancellationToken)
  {
    WorldId worldId = _context.WorldId;

    if (Guid.TryParse(idOrKey, out Guid id))
    {
      MoveId moveId = new(worldId, id);
      Move? move = await _moveRepository.LoadAsync(moveId, cancellationToken);
      if (move is not null)
      {
        return move;
      }
    }

    MoveId? foundId = await _moveQuerier.FindIdAsync(idOrKey, cancellationToken);
    if (!foundId.HasValue)
    {
      throw new MoveNotFoundException(worldId, idOrKey, propertyName);
    }

    return await _moveRepository.LoadAsync(foundId.Value, cancellationToken)
      ?? throw new InvalidOperationException($"The move 'Id={foundId}' was not loaded.");
  }
}
