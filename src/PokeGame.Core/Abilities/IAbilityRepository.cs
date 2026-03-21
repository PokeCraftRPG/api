namespace PokeGame.Core.Abilities;

public interface IAbilityRepository
{
  Task<Ability?> LoadAsync(AbilityId id, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<Ability>> LoadAsync(IEnumerable<AbilityId> ids, CancellationToken cancellationToken = default);

  Task SaveAsync(Ability ability, CancellationToken cancellationToken = default);
  Task SaveAsync(IEnumerable<Ability> abilities, CancellationToken cancellationToken = default);
}
