using Bogus;
using PokeGame.Core.Battles;

namespace PokeGame.Core.Items.Properties;

[Trait(Traits.Category, Categories.Unit)]
public class BattleItemPropertiesTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should construct an instance from another.")]
  public void Given_Instance_When_ctor_Then_CorrectProperties()
  {
    BattleItemProperties instance = new(2, -1, 3, 0, 1, -2, 4, 2, 5);
    BattleItemProperties properties = new(instance);
    Assert.Equal(instance.Attack, properties.Attack);
    Assert.Equal(instance.Defense, properties.Defense);
    Assert.Equal(instance.SpecialAttack, properties.SpecialAttack);
    Assert.Equal(instance.SpecialDefense, properties.SpecialDefense);
    Assert.Equal(instance.Speed, properties.Speed);
    Assert.Equal(instance.Accuracy, properties.Accuracy);
    Assert.Equal(instance.Evasion, properties.Evasion);
    Assert.Equal(instance.Critical, properties.Critical);
    Assert.Equal(instance.GuardTurns, properties.GuardTurns);
  }

  [Fact(DisplayName = "ctor: it should construct an instance from arguments.")]
  public void Given_ValidArguments_When_ctor_Then_CorrectProperties()
  {
    int attack = _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage);
    int defense = _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage);
    int specialAttack = _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage);
    int specialDefense = _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage);
    int speed = _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage);
    int accuracy = _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage);
    int evasion = _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage);
    int critical = _faker.Random.Int(StatisticChanges.MinimumCritical, StatisticChanges.MaximumCritical);
    int guardTurns = _faker.Random.Int(0, 10);
    BattleItemProperties properties = new(attack, defense, specialAttack, specialDefense, speed, accuracy, evasion, critical, guardTurns);
    Assert.Equal(attack, properties.Attack);
    Assert.Equal(defense, properties.Defense);
    Assert.Equal(specialAttack, properties.SpecialAttack);
    Assert.Equal(specialDefense, properties.SpecialDefense);
    Assert.Equal(speed, properties.Speed);
    Assert.Equal(accuracy, properties.Accuracy);
    Assert.Equal(evasion, properties.Evasion);
    Assert.Equal(critical, properties.Critical);
    Assert.Equal(guardTurns, properties.GuardTurns);
  }

  [Fact(DisplayName = "ctor: it should construct the default instance.")]
  public void Given_NoArgument_When_ctor_Then_DefaultInstance()
  {
    BattleItemProperties properties = new();
    Assert.Equal(0, properties.Attack);
    Assert.Equal(0, properties.Defense);
    Assert.Equal(0, properties.SpecialAttack);
    Assert.Equal(0, properties.SpecialDefense);
    Assert.Equal(0, properties.Speed);
    Assert.Equal(0, properties.Accuracy);
    Assert.Equal(0, properties.Evasion);
    Assert.Equal(0, properties.Critical);
    Assert.Equal(0, properties.GuardTurns);
  }

  [Fact(DisplayName = "ctor: it should throw ValidationException when arguments are not valid.")]
  public void Given_InvalidArguments_When_ctor_Then_ValidationException()
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new BattleItemProperties(StatisticChanges.MaximumStage + 1, 0, 0, 0, 0, 0, 0, 0, 11));
    Assert.Equal(2, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Attack");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "GuardTurns");
  }
}
