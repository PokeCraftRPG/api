using Logitar.EventSourcing;
using PokeGame.Core.Moves;

namespace PokeGame.Infrastructure.Repositories;

internal class MoveRepository : Repository, IMoveRepository
{
  public MoveRepository(IEventStore eventStore) : base(eventStore)
  {
  }

  public async Task<Move?> LoadAsync(MoveId id, CancellationToken cancellationToken)
  {
    return await LoadAsync<Move>(id.StreamId, cancellationToken);
  }
  public async Task<IReadOnlyCollection<Move>> LoadAsync(IEnumerable<MoveId> ids, CancellationToken cancellationToken)
  {
    return await LoadAsync<Move>(ids.Select(id => id.StreamId), cancellationToken);
  }

  public async Task SaveAsync(Move move, CancellationToken cancellationToken)
  {
    await base.SaveAsync(move, cancellationToken);
  }
  public async Task SaveAsync(IEnumerable<Move> moves, CancellationToken cancellationToken)
  {
    await base.SaveAsync(moves, cancellationToken);
  }
}
