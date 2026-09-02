using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Abilities.Models;

namespace PokeGame.Core.Abilities.Queries;

internal record SearchAbilitiesQuery(SearchAbilitiesPayload Payload) : IQuery<SearchResults<AbilityDto>>;

internal class SearchAbilitiesQueryHandler : IQueryHandler<SearchAbilitiesQuery, SearchResults<AbilityDto>>
{
  private readonly IAbilityQuerier _abilityQuerier;

  public SearchAbilitiesQueryHandler(IAbilityQuerier abilityQuerier)
  {
    _abilityQuerier = abilityQuerier;
  }

  public async Task<SearchResults<AbilityDto>> HandleAsync(SearchAbilitiesQuery query, CancellationToken cancellationToken)
  {
    SearchAbilitiesPayload payload = query.Payload;
    payload.Validate();

    return await _abilityQuerier.SearchAsync(payload, cancellationToken);
  }
}
