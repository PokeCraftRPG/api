using Bogus;

namespace PokeGame.Core.Battles;

[Trait(Traits.Category, Categories.Unit)]
public class StatisticChangesTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "Apply: it should apply the correct statistic changes.")]
  public void Given_StatisticChanges_When_Apply_Then_CorrectChanges()
  {
    StatisticChanges statisticChanges = new(0, 0, 0, 0, 0, 5, -5, 3);
    StatisticChanges applied = new(0, 0, 0, 0, 0, 2, -2, 2);
    StatisticChanges result = statisticChanges.Apply(applied);
    Assert.Equal(0, result.Attack);
    Assert.Equal(0, result.Defense);
    Assert.Equal(0, result.SpecialAttack);
    Assert.Equal(0, result.SpecialDefense);
    Assert.Equal(0, result.Speed);
    Assert.Equal(StatisticChanges.MaximumStage, result.Accuracy);
    Assert.Equal(StatisticChanges.MinimumStage, result.Evasion);
    Assert.Equal(StatisticChanges.MaximumCritical, result.Critical);
  }

  [Fact(DisplayName = "ctor: it should construct an instance from another instance.")]
  public void Given_Instance_When_ctor_Then_Instance()
  {
    StatisticChanges instance = new(
      _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage),
      _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage),
      _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage),
      _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage),
      _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage),
      _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage),
      _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage),
      _faker.Random.Int(StatisticChanges.MinimumCritical, StatisticChanges.MaximumCritical));
    StatisticChanges statisticChanges = new(instance);
    Assert.Equal(instance.Attack, statisticChanges.Attack);
    Assert.Equal(instance.Defense, statisticChanges.Defense);
    Assert.Equal(instance.SpecialAttack, statisticChanges.SpecialAttack);
    Assert.Equal(instance.SpecialDefense, statisticChanges.SpecialDefense);
    Assert.Equal(instance.Speed, statisticChanges.Speed);
    Assert.Equal(instance.Accuracy, statisticChanges.Accuracy);
    Assert.Equal(instance.Evasion, statisticChanges.Evasion);
    Assert.Equal(instance.Critical, statisticChanges.Critical);
  }

  [Fact(DisplayName = "ctor: it should construct an instance from arguments.")]
  public void Given_ValidArguments_When_ctor_Then_Instance()
  {
    int attack = _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage);
    int defense = _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage);
    int specialAttack = _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage);
    int specialDefense = _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage);
    int speed = _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage);
    int accuracy = _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage);
    int evasion = _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage);
    int critical = _faker.Random.Int(StatisticChanges.MinimumCritical, StatisticChanges.MaximumCritical);
    StatisticChanges statisticChanges = new(attack, defense, specialAttack, specialDefense, speed, accuracy, evasion, critical);
    Assert.Equal(attack, statisticChanges.Attack);
    Assert.Equal(defense, statisticChanges.Defense);
    Assert.Equal(specialAttack, statisticChanges.SpecialAttack);
    Assert.Equal(specialDefense, statisticChanges.SpecialDefense);
    Assert.Equal(speed, statisticChanges.Speed);
    Assert.Equal(accuracy, statisticChanges.Accuracy);
    Assert.Equal(evasion, statisticChanges.Evasion);
    Assert.Equal(critical, statisticChanges.Critical);
  }

  [Fact(DisplayName = "ctor: it should construct the default instance.")]
  public void Given_NoArgument_When_ctor_Then_DefaultInstance()
  {
    StatisticChanges changes = new();
    Assert.Equal(0, changes.Attack);
    Assert.Equal(0, changes.Defense);
    Assert.Equal(0, changes.SpecialAttack);
    Assert.Equal(0, changes.SpecialDefense);
    Assert.Equal(0, changes.Speed);
    Assert.Equal(0, changes.Accuracy);
    Assert.Equal(0, changes.Evasion);
    Assert.Equal(0, changes.Critical);
  }

  [Theory(DisplayName = "ctor: it should throw ValidationException when the arguments are not valid.")]
  [InlineData(false)]
  [InlineData(true)]
  public void Given_InvalidArguments_When_ctor_Then_ValidationException(bool isOver)
  {
    int critical = isOver ? (StatisticChanges.MaximumCritical + 1) : (StatisticChanges.MinimumCritical - 1);
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new StatisticChanges(StatisticChanges.MinimumStage - 1, 0, StatisticChanges.MaximumStage + 1, 0, 0, 0, 0, critical));

    Assert.Equal(3, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Attack");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "SpecialAttack");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Critical");
  }
}
