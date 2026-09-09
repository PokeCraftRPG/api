using FluentValidation;

namespace PokeGame.Core.Forms;

public interface IBaseStatistics
{
  byte HP { get; }
  byte Attack { get; }
  byte Defense { get; }
  byte SpecialAttack { get; }
  byte SpecialDefense { get; }
  byte Speed { get; }
}

public sealed record BaseStatistics : IBaseStatistics
{
  public byte HP { get; }
  public byte Attack { get; }
  public byte Defense { get; }
  public byte SpecialAttack { get; }
  public byte SpecialDefense { get; }
  public byte Speed { get; }

  public BaseStatistics(byte hp, byte attack, byte defense, byte specialAttack, byte specialDefense, byte speed)
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
    RuleFor(x => x.HP).GreaterThan((byte)0);
    RuleFor(x => x.Attack).GreaterThan((byte)0);
    RuleFor(x => x.Defense).GreaterThan((byte)0);
    RuleFor(x => x.SpecialAttack).GreaterThan((byte)0);
    RuleFor(x => x.SpecialDefense).GreaterThan((byte)0);
    RuleFor(x => x.Speed).GreaterThan((byte)0);
  }
}
