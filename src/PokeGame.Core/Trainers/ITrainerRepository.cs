namespace PokeGame.Core.Trainers;

public interface ITrainerRepository
{
  Task<Trainer?> LoadAsync(TrainerId id, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<Trainer>> LoadAsync(IEnumerable<TrainerId> ids, CancellationToken cancellationToken = default);

  Task SaveAsync(Trainer trainer, CancellationToken cancellationToken = default);
  Task SaveAsync(IEnumerable<Trainer> trainers, CancellationToken cancellationToken = default);
}
