using Krakenar.Contracts.Search;
using PokeGame.Core.Abilities.Models;

namespace PokeGame.Core.Abilities;

public interface IAbilityQuerier
{
  Task<AbilityId?> GetIdAsync(Key key, CancellationToken cancellationToken = default);

  Task<AbilityDto> ReadAsync(Ability ability, CancellationToken cancellationToken = default);
  Task<AbilityDto?> ReadAsync(AbilityId id, CancellationToken cancellationToken = default);
  Task<AbilityDto?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<AbilityDto?> ReadAsync(string key, CancellationToken cancellationToken = default);

  Task<SearchResults<AbilityDto>> SearchAsync(SearchAbilitiesPayload payload, CancellationToken cancellationToken = default);
}
