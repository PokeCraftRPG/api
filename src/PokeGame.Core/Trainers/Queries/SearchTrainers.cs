using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Trainers.Models;

namespace PokeGame.Core.Trainers.Queries;

internal record SearchTrainersQuery(SearchTrainersPayload Payload) : IQuery<SearchResults<TrainerDto>>;

internal class SearchTrainersQueryHandler : IQueryHandler<SearchTrainersQuery, SearchResults<TrainerDto>>
{
  private readonly ITrainerQuerier _trainerQuerier;

  public SearchTrainersQueryHandler(ITrainerQuerier trainerQuerier)
  {
    _trainerQuerier = trainerQuerier;
  }

  public async Task<SearchResults<TrainerDto>> HandleAsync(SearchTrainersQuery query, CancellationToken cancellationToken)
  {
    SearchTrainersPayload payload = query.Payload;
    payload.Validate();

    return await _trainerQuerier.SearchAsync(payload, cancellationToken);
  }
}
