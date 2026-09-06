using FluentValidation;

namespace PokeGame.Core.Forms;

public interface IFormYield
{
  int Experience { get; }

  int HP { get; }
  int Attack { get; }
  int Defense { get; }
  int SpecialAttack { get; }
  int SpecialDefense { get; }
  int Speed { get; }
}

public sealed record FormYield : IFormYield
{
  public int Experience { get; }

  public int HP { get; }
  public int Attack { get; }
  public int Defense { get; }
  public int SpecialAttack { get; }
  public int SpecialDefense { get; }
  public int Speed { get; }

  public FormYield(int experience, int hp, int attack, int defense, int specialAttack, int specialDefense, int speed)
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

    RuleFor(x => x.HP).InclusiveBetween(0, 3);
    RuleFor(x => x.Attack).InclusiveBetween(0, 3);
    RuleFor(x => x.Defense).InclusiveBetween(0, 3);
    RuleFor(x => x.SpecialAttack).InclusiveBetween(0, 3);
    RuleFor(x => x.SpecialDefense).InclusiveBetween(0, 3);
    RuleFor(x => x.Speed).InclusiveBetween(0, 3);

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
