using Krakenar.Contracts.Search;
using PokeGame.Core.Abilities.Events;
using PokeGame.Core.Abilities.Models;

namespace PokeGame.Core.Abilities;

public interface IAbilityRepository
{
  void Add(params Ability[] abilities);
  void Remove(Ability ability);
  void Update(Ability ability, AbilityUpdated record);

  Task EnsureUnicityAsync(Ability ability, CancellationToken cancellationToken = default);

  Task<Ability?> LoadAsync(Guid id, CancellationToken cancellationToken = default);

  Task<AbilityModel> ReadAsync(Ability ability, CancellationToken cancellationToken = default);
  Task<AbilityModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<AbilityModel?> ReadAsync(string key, CancellationToken cancellationToken = default);

  Task<SearchResults<AbilityModel>> SearchAsync(SearchAbilitiesPayload payload, CancellationToken cancellationToken = default);
}
