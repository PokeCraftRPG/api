using Krakenar.Contracts;
using Logitar.CQRS;
using PokeGame.Core.Trainers.Models;

namespace PokeGame.Core.Trainers.Queries;

internal record ReadTrainerQuery(Guid? Id, string? Key) : IQuery<TrainerDto?>;

internal class ReadTrainerQueryHandler : IQueryHandler<ReadTrainerQuery, TrainerDto?>
{
  private readonly ITrainerQuerier _trainerQuerier;

  public ReadTrainerQueryHandler(ITrainerQuerier trainerQuerier)
  {
    _trainerQuerier = trainerQuerier;
  }

  public async Task<TrainerDto?> HandleAsync(ReadTrainerQuery query, CancellationToken cancellationToken)
  {
    Dictionary<Guid, TrainerDto> trainers = new(capacity: 2);

    if (query.Id.HasValue)
    {
      TrainerDto? trainer = await _trainerQuerier.ReadAsync(query.Id.Value, cancellationToken);
      if (trainer is not null)
      {
        trainers[trainer.Id] = trainer;
      }
    }

    if (!string.IsNullOrWhiteSpace(query.Key))
    {
      TrainerDto? trainer = await _trainerQuerier.ReadAsync(query.Key, cancellationToken);
      if (trainer is not null)
      {
        trainers[trainer.Id] = trainer;
      }
    }

    // TODO(fpion): License

    if (trainers.Count > 1)
    {
      throw TooManyResultsException<TrainerDto>.ExpectedSingle(trainers.Count);
    }

    return trainers.Values.SingleOrDefault();
  }
}
