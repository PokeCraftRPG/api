using Krakenar.Contracts;
using Logitar.CQRS;
using PokeGame.Core.Trainers.Models;

namespace PokeGame.Core.Trainers.Queries;

internal record ReadTrainerQuery(Guid? Id, string? Key) : IQuery<TrainerModel?>; // TODO(fpion): License

internal class ReadTrainerQueryHandler : IQueryHandler<ReadTrainerQuery, TrainerModel?>
{
  private readonly ITrainerQuerier _trainerQuerier;

  public ReadTrainerQueryHandler(ITrainerQuerier trainerQuerier)
  {
    _trainerQuerier = trainerQuerier;
  }

  public async Task<TrainerModel?> HandleAsync(ReadTrainerQuery query, CancellationToken cancellationToken)
  {
    Dictionary<Guid, TrainerModel> trainers = new(capacity: 2);

    if (query.Id.HasValue)
    {
      TrainerModel? trainer = await _trainerQuerier.ReadAsync(query.Id.Value, cancellationToken);
      if (trainer is not null)
      {
        trainers[trainer.Id] = trainer;
      }
    }

    if (!string.IsNullOrWhiteSpace(query.Key))
    {
      TrainerModel? trainer = await _trainerQuerier.ReadAsync(query.Key, cancellationToken);
      if (trainer is not null)
      {
        trainers[trainer.Id] = trainer;
      }
    }

    if (trainers.Count > 1)
    {
      throw TooManyResultsException<TrainerModel>.ExpectedSingle(trainers.Count);
    }

    return trainers.Values.SingleOrDefault();
  }
}
