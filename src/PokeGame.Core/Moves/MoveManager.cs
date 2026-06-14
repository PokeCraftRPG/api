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
    Slug? key = null;

    foreach (IEvent change in move.Changes)
    {
      if (change is MoveCreated || change is MoveKeyChanged)
      {
        key = move.Key;
      }
    }

    if (key is not null)
    {
      MoveId? otherId = await _moveQuerier.TryGetIdAsync(key, cancellationToken);
      if (otherId.HasValue && !otherId.Value.Equals(move.Id))
      {
        throw new KeyAlreadyUsedException(move, otherId.Value.EntityId, key, nameof(move.Key));
      }
    }
  }
}
