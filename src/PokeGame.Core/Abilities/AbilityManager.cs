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
    Key? key = null;
    foreach (IEvent change in ability.Changes)
    {
      if (change is AbilityCreated created)
      {
        key = created.Key;
      }
      else if (change is AbilityKeyChanged changed)
      {
        key = changed.Key;
      }
    }

    if (key is not null)
    {
      AbilityId? abilityId = await _abilityQuerier.GetIdAsync(key, cancellationToken);
      if (abilityId.HasValue && !abilityId.Value.Equals(ability.Id))
      {
        throw new KeyAlreadyUsedException(ability, abilityId.Value.EntityId, ability.Key, nameof(ability.Key));
      }
    }
  }
}
