namespace PokeGame.Core.Abilities.Events;

public class AbilityUpdated : UpdateEvent
{
  public Change<string>? Key { get; set; }
  public Change<string>? Name { get; set; }
  public Change<string>? Description { get; set; }

  public AbilityUpdated() : base()
  {
  }

  public AbilityUpdated(Ability ability) : base(ability)
  {
  }
}
