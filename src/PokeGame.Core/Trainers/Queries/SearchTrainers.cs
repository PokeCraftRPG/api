using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Trainers.Models;

namespace PokeGame.Core.Trainers.Queries;

internal record SearchTrainersQuery(SearchTrainersPayload Payload) : IQuery<SearchResults<TrainerModel>>;

internal class SearchTrainersQueryHandler : IQueryHandler<SearchTrainersQuery, SearchResults<TrainerModel>>
{
  private readonly ITrainerQuerier _trainerQuerier;

  public SearchTrainersQueryHandler(ITrainerQuerier trainerQuerier)
  {
    _trainerQuerier = trainerQuerier;
  }

  public async Task<SearchResults<TrainerModel>> HandleAsync(SearchTrainersQuery query, CancellationToken cancellationToken)
  {
    return await _trainerQuerier.SearchAsync(query.Payload, cancellationToken);
  }
}
