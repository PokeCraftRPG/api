using Logitar.EventSourcing;
using PokeGame.Core.Rosters;
using PokeGame.Core.Trainers;

namespace PokeGame.Infrastructure.Repositories;

internal class RosterRepository : Repository, IRosterRepository
{
  public RosterRepository(IEventStore eventStore) : base(eventStore)
  {
  }

  public async Task<Roster> LoadAsync(Trainer trainer, CancellationToken cancellationToken)
  {
    return await LoadAsync(trainer.Id, cancellationToken);
  }
  public async Task<Roster> LoadAsync(TrainerId trainerId, CancellationToken cancellationToken)
  {
    return await LoadAsync(new RosterId(trainerId), cancellationToken) ?? new(trainerId);
  }
  public async Task<Roster?> LoadAsync(RosterId id, CancellationToken cancellationToken)
  {
    return await LoadAsync<Roster>(id.StreamId, cancellationToken);
  }
  public async Task<IReadOnlyCollection<Roster>> LoadAsync(IEnumerable<RosterId> ids, CancellationToken cancellationToken)
  {
    return await LoadAsync<Roster>(ids.Select(id => id.StreamId), cancellationToken);
  }

  public async Task SaveAsync(Roster roster, CancellationToken cancellationToken)
  {
    await base.SaveAsync(roster, cancellationToken);
  }
  public async Task SaveAsync(IEnumerable<Roster> rosters, CancellationToken cancellationToken)
  {
    await base.SaveAsync(rosters, cancellationToken);
  }
}
