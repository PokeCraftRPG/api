namespace PokeGame.Core.Abilities.Events;

public class AbilityCreated : CreateEvent
{
  public string Key { get; set; } = string.Empty;
  public string? Name { get; set; }
  public string? Description { get; set; }

  public AbilityCreated() : base()
  {
  }

  public AbilityCreated(Ability ability) : base(ability)
  {
    Key = ability.Key;
    Name = ability.Name;
    Description = ability.Description;
  }
}
