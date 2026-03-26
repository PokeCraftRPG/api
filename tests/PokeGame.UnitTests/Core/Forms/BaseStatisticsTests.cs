namespace PokeGame.Core.Forms;

[Trait(Traits.Category, Categories.Unit)]
public class BaseStatisticsTests
{
  [Fact(DisplayName = "It should construct BaseStatistics from another instance.")]
  public void Given_Instance_When_ctor_Then_BaseStatistics()
  {
    BaseStatistics instance = new(35, 55, 40, 50, 50, 90);
    BaseStatistics baseStatistics = new(instance);
    Assert.Equal(instance.HP, baseStatistics.HP);
    Assert.Equal(instance.Attack, baseStatistics.Attack);
    Assert.Equal(instance.Defense, baseStatistics.Defense);
    Assert.Equal(instance.SpecialAttack, baseStatistics.SpecialAttack);
    Assert.Equal(instance.SpecialDefense, baseStatistics.SpecialDefense);
    Assert.Equal(instance.Speed, baseStatistics.Speed);
  }

  [Fact(DisplayName = "It should construct BaseStatistics from valid arguments.")]
  public void Given_ValidArguments_When_ctor_Then_BaseStatistics()
  {
    byte hp = 35;
    byte attack = 55;
    byte defense = 40;
    byte specialAttack = 50;
    byte specialDefense = 50;
    byte speed = 90;
    BaseStatistics baseStatistics = new(hp, attack, defense, specialAttack, specialDefense, speed);
    Assert.Equal(hp, baseStatistics.HP);
    Assert.Equal(attack, baseStatistics.Attack);
    Assert.Equal(defense, baseStatistics.Defense);
    Assert.Equal(specialAttack, baseStatistics.SpecialAttack);
    Assert.Equal(specialDefense, baseStatistics.SpecialDefense);
    Assert.Equal(speed, baseStatistics.Speed);
  }

  [Fact(DisplayName = "It should throw ValidationException when the arguments are not valid.")]
  public void Given_InvalidArguments_When_ctor_Then_ValidationException()
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new BaseStatistics(0, 0, 0, 0, 0, 0));
    Assert.Equal(6, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "HP");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "Attack");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "Defense");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "SpecialAttack");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "SpecialDefense");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "Speed");
  }
}
