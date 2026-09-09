using FluentValidation;

namespace PokeGame.Core.Specimens;

public interface IIndividualValues
{
  int HP { get; }
  int Attack { get; }
  int Defense { get; }
  int SpecialAttack { get; }
  int SpecialDefense { get; }
  int Speed { get; }
}

public sealed record IndividualValues : IIndividualValues
{
  public int HP { get; }
  public int Attack { get; }
  public int Defense { get; }
  public int SpecialAttack { get; }
  public int SpecialDefense { get; }
  public int Speed { get; }

  public IndividualValues(int hp, int attack, int defense, int specialAttack, int specialDefense, int speed)
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
    RuleFor(x => x.HP).InclusiveBetween(0, 31);
    RuleFor(x => x.Attack).InclusiveBetween(0, 31);
    RuleFor(x => x.Defense).InclusiveBetween(0, 31);
    RuleFor(x => x.SpecialAttack).InclusiveBetween(0, 31);
    RuleFor(x => x.SpecialDefense).InclusiveBetween(0, 31);
    RuleFor(x => x.Speed).InclusiveBetween(0, 31);
  }
}
