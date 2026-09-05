using PokeGame.Core.Abilities.Models;

namespace PokeGame.Core.Forms.Models;

public class FormAbilitiesDto
{
  public AbilityDto Primary { get; set; } = new();
  public AbilityDto? Secondary { get; set; }
  public AbilityDto? Hidden { get; set; }
}
