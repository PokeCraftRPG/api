using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Evolutions.Models;

namespace PokeGame.Core.Evolutions.Queries;

internal record SearchEvolutionsQuery(SearchEvolutionsPayload Payload) : IQuery<SearchResults<EvolutionModel>>;

internal class SearchEvolutionsQueryHandler : IQueryHandler<SearchEvolutionsQuery, SearchResults<EvolutionModel>>
{
  private readonly IEvolutionQuerier _evolutionQuerier;

  public SearchEvolutionsQueryHandler(IEvolutionQuerier evolutionQuerier)
  {
    _evolutionQuerier = evolutionQuerier;
  }

  public async Task<SearchResults<EvolutionModel>> HandleAsync(SearchEvolutionsQuery query, CancellationToken cancellationToken)
  {
    return await _evolutionQuerier.SearchAsync(query.Payload, cancellationToken);
  }
}
