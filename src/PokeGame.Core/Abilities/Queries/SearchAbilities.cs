using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Abilities.Models;

namespace PokeGame.Core.Abilities.Queries;

internal record SearchAbilitiesQuery(SearchAbilitiesPayload Payload) : IQuery<SearchResults<AbilityModel>>;

internal class SearchAbilitiesQueryHandler : IQueryHandler<SearchAbilitiesQuery, SearchResults<AbilityModel>>
{
  private readonly IAbilityRepository _abilityRepository;

  public SearchAbilitiesQueryHandler(IAbilityRepository abilityRepository)
  {
    _abilityRepository = abilityRepository;
  }

  public async Task<SearchResults<AbilityModel>> HandleAsync(SearchAbilitiesQuery query, CancellationToken cancellationToken)
  {
    return await _abilityRepository.SearchAsync(query.Payload, cancellationToken);
  }
}
