namespace PokeGame.Core.Abilities.Events;

public class AbilityDeleted : DeleteEvent
{
  public AbilityDeleted() : base()
  {
  }

  public AbilityDeleted(Ability ability, Guid userId) : base(ability, userId)
  {
  }
}
