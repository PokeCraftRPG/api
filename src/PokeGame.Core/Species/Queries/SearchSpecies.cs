using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species.Queries;

internal record SearchSpeciesQuery(SearchSpeciesPayload Payload) : IQuery<SearchResults<SpeciesModel>>;

internal class SearchSpeciesQueryHandler : IQueryHandler<SearchSpeciesQuery, SearchResults<SpeciesModel>>
{
  private readonly ISpeciesRepository _speciesRepository;

  public SearchSpeciesQueryHandler(ISpeciesRepository speciesRepository)
  {
    _speciesRepository = speciesRepository;
  }

  public async Task<SearchResults<SpeciesModel>> HandleAsync(SearchSpeciesQuery query, CancellationToken cancellationToken)
  {
    return await _speciesRepository.SearchAsync(query.Payload, cancellationToken);
  }
}
