using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Abilities.Models;

namespace PokeGame.Core.Abilities.Queries;

internal record SearchAbilitiesQuery(SearchAbilitiesPayload Payload) : IQuery<SearchResults<AbilityModel>>;

internal class SearchAbilitiesQueryHandler : IQueryHandler<SearchAbilitiesQuery, SearchResults<AbilityModel>>
{
  private readonly IAbilityQuerier _abilityQuerier;

  public SearchAbilitiesQueryHandler(IAbilityQuerier abilityQuerier)
  {
    _abilityQuerier = abilityQuerier;
  }

  public async Task<SearchResults<AbilityModel>> HandleAsync(SearchAbilitiesQuery query, CancellationToken cancellationToken)
  {
    return await _abilityQuerier.SearchAsync(query.Payload, cancellationToken);
  }
}
