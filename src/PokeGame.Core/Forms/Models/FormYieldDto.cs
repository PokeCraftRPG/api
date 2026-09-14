namespace PokeGame.Core.Forms.Models;

public record FormYieldDto : IFormYield
{
  public int Experience { get; set; }

  public byte HP { get; set; }
  public byte Attack { get; set; }
  public byte Defense { get; set; }
  public byte SpecialAttack { get; set; }
  public byte SpecialDefense { get; set; }
  public byte Speed { get; set; }

  public FormYieldDto()
  {
  }

  public FormYieldDto(int experience, byte hp, byte attack, byte defense, byte specialAttack, byte specialDefense, byte speed)
  {
    Experience = experience;

    HP = hp;
    Attack = attack;
    Defense = defense;
    SpecialAttack = specialAttack;
    SpecialDefense = specialDefense;
    Speed = speed;
  }

  public FormYieldDto(IFormYield yield) : this(yield.Experience, yield.HP, yield.Attack, yield.Defense, yield.SpecialAttack, yield.SpecialDefense, yield.Speed)
  {
  }
}
