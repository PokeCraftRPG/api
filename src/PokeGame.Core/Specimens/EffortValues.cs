using FluentValidation;

namespace PokeGame.Core.Specimens;

public interface IEffortValues
{
  int HP { get; }
  int Attack { get; }
  int Defense { get; }
  int SpecialAttack { get; }
  int SpecialDefense { get; }
  int Speed { get; }
}

public sealed record EffortValues : IEffortValues
{
  public int HP { get; }
  public int Attack { get; }
  public int Defense { get; }
  public int SpecialAttack { get; }
  public int SpecialDefense { get; }
  public int Speed { get; }

  public EffortValues(int hp = 0, int attack = 0, int defense = 0, int specialAttack = 0, int specialDefense = 0, int speed = 0)
  {
    HP = hp;
    Attack = attack;
    Defense = defense;
    SpecialAttack = specialAttack;
    SpecialDefense = specialDefense;
    Speed = speed;
    new EffortValuesValidator().ValidateAndThrow(this);
  }

  public static IndividualValues From(IEffortValues effort)
    => new(effort.HP, effort.Attack, effort.Defense, effort.SpecialAttack, effort.SpecialDefense, effort.Speed);
}

internal class EffortValuesValidator : AbstractValidator<IEffortValues>
{
  private const int MaximumTotal = 510;

  public EffortValuesValidator()
  {
    RuleFor(x => x.HP).InclusiveBetween(0, byte.MaxValue);
    RuleFor(x => x.Attack).InclusiveBetween(0, byte.MaxValue);
    RuleFor(x => x.Defense).InclusiveBetween(0, byte.MaxValue);
    RuleFor(x => x.SpecialAttack).InclusiveBetween(0, byte.MaxValue);
    RuleFor(x => x.SpecialDefense).InclusiveBetween(0, byte.MaxValue);
    RuleFor(x => x.Speed).InclusiveBetween(0, byte.MaxValue);

    RuleFor(x => x).Must(HaveAValidTotal)
      .WithErrorCode(nameof(EffortValuesValidator))
      .WithMessage($"The total effort values must be less than or equal to {MaximumTotal}.");
  }

  private static bool HaveAValidTotal(IEffortValues effort)
  {
    int total = effort.HP + effort.Attack + effort.Defense + effort.SpecialAttack + effort.SpecialDefense + effort.Speed;
    return total <= MaximumTotal;
  }
}
