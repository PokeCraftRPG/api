using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Rosters.Models;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Rosters.Queries;

internal record SearchTagsQuery(Guid TrainerId) : IQuery<SearchResults<TagDto>?>;

internal class SearchTagsQueryHandler : IQueryHandler<SearchTagsQuery, SearchResults<TagDto>?>
{
  private readonly IRosterQuerier _rosterQuerier;
  private readonly ITrainerQuerier _trainerQuerier;

  public SearchTagsQueryHandler(IRosterQuerier rosterQuerier, ITrainerQuerier trainerQuerier)
  {
    _rosterQuerier = rosterQuerier;
    _trainerQuerier = trainerQuerier;
  }

  public async Task<SearchResults<TagDto>?> HandleAsync(SearchTagsQuery query, CancellationToken cancellationToken)
  {
    if (!await _trainerQuerier.ExistsAsync(query.TrainerId, cancellationToken))
    {
      return null;
    }

    return await _rosterQuerier.SearchTagsAsync(query.TrainerId, cancellationToken);
  }
}
