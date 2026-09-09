using FluentValidation;

namespace PokeGame.Core.Forms;

public interface IFormYield
{
  int Experience { get; }

  byte HP { get; }
  byte Attack { get; }
  byte Defense { get; }
  byte SpecialAttack { get; }
  byte SpecialDefense { get; }
  byte Speed { get; }
}

public sealed record FormYield : IFormYield
{
  public int Experience { get; }

  public byte HP { get; }
  public byte Attack { get; }
  public byte Defense { get; }
  public byte SpecialAttack { get; }
  public byte SpecialDefense { get; }
  public byte Speed { get; }

  public FormYield(int experience, byte hp, byte attack, byte defense, byte specialAttack, byte specialDefense, byte speed)
  {
    Experience = experience;

    HP = hp;
    Attack = attack;
    Defense = defense;
    SpecialAttack = specialAttack;
    SpecialDefense = specialDefense;
    Speed = speed;

    new FormYieldValidator().ValidateAndThrow(this);
  }

  public static FormYield From(IFormYield yield) => new(yield.Experience, yield.HP, yield.Attack, yield.Defense, yield.SpecialAttack, yield.SpecialDefense, yield.Speed);
}

internal class FormYieldValidator : AbstractValidator<IFormYield>
{
  public FormYieldValidator()
  {
    RuleFor(x => x.Experience).InclusiveBetween(1, 999);

    RuleFor(x => x.HP).InclusiveBetween((byte)0, (byte)3);
    RuleFor(x => x.Attack).InclusiveBetween((byte)0, (byte)3);
    RuleFor(x => x.Defense).InclusiveBetween((byte)0, (byte)3);
    RuleFor(x => x.SpecialAttack).InclusiveBetween((byte)0, (byte)3);
    RuleFor(x => x.SpecialDefense).InclusiveBetween((byte)0, (byte)3);
    RuleFor(x => x.Speed).InclusiveBetween((byte)0, (byte)3);

    RuleFor(x => x).Must(HaveAValidTotal)
      .WithErrorCode(nameof(FormYieldValidator))
      .WithMessage("The total Effort Value yield must be comprised between 1 and 4.");
  }

  private static bool HaveAValidTotal(IFormYield yield)
  {
    int total = yield.HP + yield.Attack + yield.Defense + yield.SpecialAttack + yield.SpecialDefense + yield.Speed;
    return total >= 1 && total <= 4;
  }
}
