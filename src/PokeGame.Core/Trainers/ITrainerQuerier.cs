using Krakenar.Contracts.Search;
using PokeGame.Core.Identity;
using PokeGame.Core.Trainers.Models;

namespace PokeGame.Core.Trainers;

public interface ITrainerQuerier
{
  Task<TrainerId?> GetIdAsync(Key key, CancellationToken cancellationToken = default);
  Task<TrainerId?> GetIdAsync(License license, CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<TrainerId>> ListIdsAsync(UserId memberId, CancellationToken cancellationToken = default);

  Task<TrainerDto> ReadAsync(Trainer trainer, CancellationToken cancellationToken = default);
  Task<TrainerDto?> ReadAsync(TrainerId id, CancellationToken cancellationToken = default);
  Task<TrainerDto?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<TrainerDto?> ReadAsync(string key, CancellationToken cancellationToken = default);
  Task<TrainerDto?> ReadByLicenseAsync(string license, CancellationToken cancellationToken = default);

  Task<SearchResults<TrainerDto>> SearchAsync(SearchTrainersPayload payload, CancellationToken cancellationToken = default);
}
