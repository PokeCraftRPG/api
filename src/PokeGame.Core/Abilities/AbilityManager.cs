using Logitar.EventSourcing;
using PokeGame.Core.Abilities.Events;

namespace PokeGame.Core.Abilities;

public interface IAbilityManager
{
  Task EnsureUnicityAsync(Ability ability, CancellationToken cancellationToken = default);
}

internal class AbilityManager : IAbilityManager
{
  private readonly IAbilityQuerier _abilityQuerier;

  public AbilityManager(IAbilityQuerier abilityQuerier)
  {
    _abilityQuerier = abilityQuerier;
  }

  public async Task EnsureUnicityAsync(Ability ability, CancellationToken cancellationToken)
  {
    Slug? key = null;

    foreach (IEvent change in ability.Changes)
    {
      if (change is AbilityCreated || change is AbilityKeyChanged)
      {
        key = ability.Key;
      }
    }

    if (key is not null)
    {
      AbilityId? otherId = await _abilityQuerier.TryGetIdAsync(key, cancellationToken);
      if (otherId.HasValue && !otherId.Value.Equals(ability.Id))
      {
        throw new KeyAlreadyUsedException(ability, otherId.Value.EntityId, key, nameof(ability.Key));
      }
    }
  }
}
