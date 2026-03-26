namespace PokeGame.Core.Forms;

[Trait(Traits.Category, Categories.Unit)]
public class YieldTests
{
  [Fact(DisplayName = "It should construct Yield from another instance.")]
  public void Given_Instance_When_ctor_Then_Yield()
  {
    Yield instance = new(172, 0, 0, 0, 0, 0, 2);
    Yield yield = new(instance);
    Assert.Equal(instance.Experience, yield.Experience);
    Assert.Equal(instance.HP, yield.HP);
    Assert.Equal(instance.Attack, yield.Attack);
    Assert.Equal(instance.Defense, yield.Defense);
    Assert.Equal(instance.SpecialAttack, yield.SpecialAttack);
    Assert.Equal(instance.SpecialDefense, yield.SpecialDefense);
    Assert.Equal(instance.Speed, yield.Speed);
  }

  [Fact(DisplayName = "It should construct Yield from valid arguments.")]
  public void Given_ValidArguments_When_ctor_Then_Yield()
  {
    int experience = 112;
    int hp = 0;
    int attack = 0;
    int defense = 0;
    int specialAttack = 0;
    int specialDefense = 0;
    int speed = 2;
    Yield yield = new(experience, hp, attack, defense, specialAttack, specialDefense, speed);
    Assert.Equal(experience, yield.Experience);
    Assert.Equal(hp, yield.HP);
    Assert.Equal(attack, yield.Attack);
    Assert.Equal(defense, yield.Defense);
    Assert.Equal(specialAttack, yield.SpecialAttack);
    Assert.Equal(specialDefense, yield.SpecialDefense);
    Assert.Equal(speed, yield.Speed);
  }

  [Fact(DisplayName = "It should throw ValidationException when the arguments are not valid.")]
  public void Given_InvalidArguments_When_ctor_Then_ValidationException()
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new Yield(0, -1, 4, -2, 5, -3, 6));
    Assert.Equal(8, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "Experience");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "HP");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Attack");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Defense");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "SpecialAttack");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "SpecialDefense");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Speed");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "YieldValidator" && e.ErrorMessage == "The total Effort Value (EV) yield should vary from 1 to 4.");
  }
}
