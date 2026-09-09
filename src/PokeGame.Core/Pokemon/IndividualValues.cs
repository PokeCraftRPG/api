using FluentValidation;

namespace PokeGame.Core.Pokemon;

public interface IIndividualValues
{
  byte HP { get; }
  byte Attack { get; }
  byte Defense { get; }
  byte SpecialAttack { get; }
  byte SpecialDefense { get; }
  byte Speed { get; }
}

public sealed record IndividualValues : IIndividualValues
{
  public byte HP { get; }
  public byte Attack { get; }
  public byte Defense { get; }
  public byte SpecialAttack { get; }
  public byte SpecialDefense { get; }
  public byte Speed { get; }

  public IndividualValues(byte hp, byte attack, byte defense, byte specialAttack, byte specialDefense, byte speed)
  {
    HP = hp;
    Attack = attack;
    Defense = defense;
    SpecialAttack = specialAttack;
    SpecialDefense = specialDefense;
    Speed = speed;
    new IndividualValuesValidator().ValidateAndThrow(this);
  }

  public static IndividualValues From(IIndividualValues individual)
    => new(individual.HP, individual.Attack, individual.Defense, individual.SpecialAttack, individual.SpecialDefense, individual.Speed);
}

internal class IndividualValuesValidator : AbstractValidator<IIndividualValues>
{
  public IndividualValuesValidator()
  {
    RuleFor(x => x.HP).InclusiveBetween((byte)0, (byte)31);
    RuleFor(x => x.Attack).InclusiveBetween((byte)0, (byte)31);
    RuleFor(x => x.Defense).InclusiveBetween((byte)0, (byte)31);
    RuleFor(x => x.SpecialAttack).InclusiveBetween((byte)0, (byte)31);
    RuleFor(x => x.SpecialDefense).InclusiveBetween((byte)0, (byte)31);
    RuleFor(x => x.Speed).InclusiveBetween((byte)0, (byte)31);
  }
}
