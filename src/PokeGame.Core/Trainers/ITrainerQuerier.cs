using Krakenar.Contracts.Search;
using PokeGame.Core.Trainers.Models;

namespace PokeGame.Core.Trainers;

public interface ITrainerQuerier
{
  Task EnsureUnicityAsync(Trainer trainer, CancellationToken cancellationToken = default);

  Task<TrainerModel> ReadAsync(Trainer trainer, CancellationToken cancellationToken = default);
  Task<TrainerModel?> ReadAsync(TrainerId id, CancellationToken cancellationToken = default);
  Task<TrainerModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<TrainerModel?> ReadAsync(string key, CancellationToken cancellationToken = default);
  Task<TrainerModel?> ReadByLicenseAsync(string key, CancellationToken cancellationToken = default);

  Task<SearchResults<TrainerModel>> SearchAsync(SearchTrainersPayload payload, CancellationToken cancellationToken = default);
}
