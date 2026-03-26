using PokeGame.Core.Abilities.Models;

namespace PokeGame.Core.Abilities;

public interface IAbilityQuerier
{
  Task EnsureUnicityAsync(Ability ability, CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<AbilityKey>> ListKeysAsync(CancellationToken cancellationToken = default);

  Task<AbilityModel> ReadAsync(Ability ability, CancellationToken cancellationToken = default);
  Task<AbilityModel?> ReadAsync(AbilityId id, CancellationToken cancellationToken = default);
  Task<AbilityModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<AbilityModel?> ReadAsync(string key, CancellationToken cancellationToken = default);
}
