using Krakenar.Contracts.Search;
using PokeGame.Core.Abilities.Models;

namespace PokeGame.Core.Abilities;

public interface IAbilityQuerier
{
  Task<AbilityModel> FindAsync(Ability ability, CancellationToken cancellationToken = default);
  Task<AbilityModel> FindAsync(AbilityId id, CancellationToken cancellationToken = default);

  Task<AbilityModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<AbilityModel?> ReadAsync(string key, CancellationToken cancellationToken = default);

  Task<SearchResults<AbilityModel>> SearchAsync(SearchAbilitiesPayload payload, CancellationToken cancellationToken = default);

  Task<AbilityId?> TryGetIdAsync(Slug key, CancellationToken cancellationToken = default);
}
