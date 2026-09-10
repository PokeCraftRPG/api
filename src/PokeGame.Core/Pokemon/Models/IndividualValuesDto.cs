namespace PokeGame.Core.Pokemon.Models;

public record IndividualValuesDto : IIndividualValues
{
  public byte HP { get; set; }
  public byte Attack { get; set; }
  public byte Defense { get; set; }
  public byte SpecialAttack { get; set; }
  public byte SpecialDefense { get; set; }
  public byte Speed { get; set; }
}
