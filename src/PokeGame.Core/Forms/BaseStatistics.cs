using FluentValidation;

namespace PokeGame.Core.Forms;

public interface IBaseStatistics
{
  int HP { get; }
  int Attack { get; }
  int Defense { get; }
  int SpecialAttack { get; }
  int SpecialDefense { get; }
  int Speed { get; }
}

public sealed record BaseStatistics : IBaseStatistics
{
  public int HP { get; }
  public int Attack { get; }
  public int Defense { get; }
  public int SpecialAttack { get; }
  public int SpecialDefense { get; }
  public int Speed { get; }

  public BaseStatistics(int hp, int attack, int defense, int specialAttack, int specialDefense, int speed)
  {
    HP = hp;
    Attack = attack;
    Defense = defense;
    SpecialAttack = specialAttack;
    SpecialDefense = specialDefense;
    Speed = speed;
    new BaseStatisticsValidator().ValidateAndThrow(this);
  }

  public static BaseStatistics From(IBaseStatistics @base) => new(@base.HP, @base.Attack, @base.Defense, @base.SpecialAttack, @base.SpecialDefense, @base.Speed);
}

internal class BaseStatisticsValidator : AbstractValidator<IBaseStatistics>
{
  public BaseStatisticsValidator()
  {
    RuleFor(x => x.HP).InclusiveBetween(1, byte.MaxValue);
    RuleFor(x => x.Attack).InclusiveBetween(1, byte.MaxValue);
    RuleFor(x => x.Defense).InclusiveBetween(1, byte.MaxValue);
    RuleFor(x => x.SpecialAttack).InclusiveBetween(1, byte.MaxValue);
    RuleFor(x => x.SpecialDefense).InclusiveBetween(1, byte.MaxValue);
    RuleFor(x => x.Speed).InclusiveBetween(1, byte.MaxValue);
  }
}
