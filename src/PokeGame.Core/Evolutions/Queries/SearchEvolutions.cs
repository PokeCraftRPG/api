using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Evolutions.Models;

namespace PokeGame.Core.Evolutions.Queries;

internal record SearchEvolutionsQuery(SearchEvolutionsPayload Payload) : IQuery<SearchResults<EvolutionDto>>;

internal class SearchEvolutionsQueryHandler : IQueryHandler<SearchEvolutionsQuery, SearchResults<EvolutionDto>>
{
  private readonly IEvolutionQuerier _evolutionQuerier;

  public SearchEvolutionsQueryHandler(IEvolutionQuerier evolutionQuerier)
  {
    _evolutionQuerier = evolutionQuerier;
  }

  public async Task<SearchResults<EvolutionDto>> HandleAsync(SearchEvolutionsQuery query, CancellationToken cancellationToken)
  {
    SearchEvolutionsPayload payload = query.Payload;
    payload.Validate();

    return await _evolutionQuerier.SearchAsync(payload, cancellationToken);
  }
}
