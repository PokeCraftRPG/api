using PokeGame.Core.Trainers;

namespace PokeGame.Core.Rosters;

public interface IRosterRepository
{
  Task<Roster> LoadAsync(Trainer trainer, CancellationToken cancellationToken = default);
  Task<Roster> LoadAsync(TrainerId trainerId, CancellationToken cancellationToken = default);
  Task<Roster?> LoadAsync(RosterId id, CancellationToken cancellationToken = default); // TODO(fpion): migrate calls from this to the previous method
  Task<IReadOnlyCollection<Roster>> LoadAsync(IEnumerable<RosterId> ids, CancellationToken cancellationToken = default);

  Task SaveAsync(Roster roster, CancellationToken cancellationToken = default);
  Task SaveAsync(IEnumerable<Roster> rosters, CancellationToken cancellationToken = default);
}
