using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species.Queries;

internal record SearchSpeciesQuery(SearchSpeciesPayload Payload) : IQuery<SearchResults<SpeciesDto>>;

internal class SearchSpeciesQueryHandler : IQueryHandler<SearchSpeciesQuery, SearchResults<SpeciesDto>>
{
  private readonly ISpeciesQuerier _speciesQuerier;

  public SearchSpeciesQueryHandler(ISpeciesQuerier speciesQuerier)
  {
    _speciesQuerier = speciesQuerier;
  }

  public async Task<SearchResults<SpeciesDto>> HandleAsync(SearchSpeciesQuery query, CancellationToken cancellationToken)
  {
    SearchSpeciesPayload payload = query.Payload;
    payload.Validate();

    return await _speciesQuerier.SearchAsync(payload, cancellationToken);
  }
}
