using Logitar.EventSourcing;
using PokeGame.Core.Trainers;

namespace PokeGame.Infrastructure.Repositories;

internal class TrainerRepository : Repository, ITrainerRepository
{
  public TrainerRepository(IEventStore eventStore) : base(eventStore)
  {
  }

  public async Task<Trainer?> LoadAsync(TrainerId id, CancellationToken cancellationToken)
  {
    return await LoadAsync<Trainer>(id.StreamId, cancellationToken);
  }
  public async Task<IReadOnlyCollection<Trainer>> LoadAsync(IEnumerable<TrainerId> ids, CancellationToken cancellationToken)
  {
    return await LoadAsync<Trainer>(ids.Select(id => id.StreamId), cancellationToken);
  }

  public async Task SaveAsync(Trainer trainer, CancellationToken cancellationToken)
  {
    await base.SaveAsync(trainer, cancellationToken);
  }
  public async Task SaveAsync(IEnumerable<Trainer> trainers, CancellationToken cancellationToken)
  {
    await base.SaveAsync(trainers, cancellationToken);
  }
}
