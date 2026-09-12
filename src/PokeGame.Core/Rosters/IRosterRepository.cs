namespace PokeGame.Core.Rosters;

public interface IRosterRepository
{
  Task<Roster?> LoadAsync(RosterId id, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<Roster>> LoadAsync(IEnumerable<RosterId> ids, CancellationToken cancellationToken = default);

  Task SaveAsync(Roster roster, CancellationToken cancellationToken = default);
  Task SaveAsync(IEnumerable<Roster> rosters, CancellationToken cancellationToken = default);
}
