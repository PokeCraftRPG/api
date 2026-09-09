using FluentValidation;

namespace PokeGame.Core.Pokemon;

public interface IEffortValues
{
  byte HP { get; }
  byte Attack { get; }
  byte Defense { get; }
  byte SpecialAttack { get; }
  byte SpecialDefense { get; }
  byte Speed { get; }
}

public sealed record EffortValues : IEffortValues
{
  public byte HP { get; }
  public byte Attack { get; }
  public byte Defense { get; }
  public byte SpecialAttack { get; }
  public byte SpecialDefense { get; }
  public byte Speed { get; }

  public EffortValues(byte hp = 0, byte attack = 0, byte defense = 0, byte specialAttack = 0, byte specialDefense = 0, byte speed = 0)
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
