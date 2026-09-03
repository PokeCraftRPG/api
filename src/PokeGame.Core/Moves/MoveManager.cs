using Logitar.EventSourcing;
using PokeGame.Core.Moves.Events;

namespace PokeGame.Core.Moves;

public interface IMoveManager
{
  Task EnsureUnicityAsync(Move move, CancellationToken cancellationToken = default);
}

internal class MoveManager : IMoveManager
{
  private readonly IMoveQuerier _moveQuerier;

  public MoveManager(IMoveQuerier moveQuerier)
  {
    _moveQuerier = moveQuerier;
  }

  public async Task EnsureUnicityAsync(Move move, CancellationToken cancellationToken)
  {
    Key? key = null;
    foreach (IEvent change in move.Changes)
    {
      if (change is MoveCreated created)
      {
        key = created.Key;
      }
      else if (change is MoveKeyChanged changed)
      {
        key = changed.Key;
      }
    }

    if (key is not null)
    {
      MoveId? moveId = await _moveQuerier.GetIdAsync(key, cancellationToken);
      if (moveId.HasValue && !moveId.Value.Equals(move.Id))
      {
        throw new KeyAlreadyUsedException(move, moveId.Value.EntityId, move.Key, nameof(move.Key));
      }
    }
  }
}
