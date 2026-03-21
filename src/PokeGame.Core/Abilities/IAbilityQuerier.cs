using PokeGame.Core.Abilities.Models;

namespace PokeGame.Core.Abilities;

public interface IAbilityQuerier
{
  Task<AbilityModel> ReadAsync(Ability ability, CancellationToken cancellationToken = default);
  Task<AbilityModel?> ReadAsync(AbilityId id, CancellationToken cancellationToken = default);
  Task<AbilityModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  // TODO(fpion): Task<AbilityModel?> ReadAsync(string slug, CancellationToken cancellationToken = default);
}
