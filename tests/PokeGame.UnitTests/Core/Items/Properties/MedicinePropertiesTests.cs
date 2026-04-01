using Bogus;
using PokeGame.Core.Moves;

namespace PokeGame.Core.Items.Properties;

[Trait(Traits.Category, Categories.Unit)]
public class MedicinePropertiesTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should construct an instance from another.")]
  public void Given_Instance_When_ctor_Then_CorrectProperties()
  {
    MedicineProperties instance = new(
      isHerbal: true,
      healing: 35,
      isHealingPercentage: true,
      revives: false,
      statusCondition: StatusCondition.Sleep,
      allConditions: false,
      powerPoints: 10,
      isPowerPointPercentage: false,
      restoreAllMoves: false);
    MedicineProperties properties = new(instance);
    Assert.Equal(instance.IsHerbal, properties.IsHerbal);
    Assert.Equal(instance.Healing, properties.Healing);
    Assert.Equal(instance.IsHealingPercentage, properties.IsHealingPercentage);
    Assert.Equal(instance.Revives, properties.Revives);
    Assert.Equal(instance.StatusCondition, properties.StatusCondition);
    Assert.Equal(instance.AllConditions, properties.AllConditions);
    Assert.Equal(instance.PowerPoints, properties.PowerPoints);
    Assert.Equal(instance.IsPowerPointPercentage, properties.IsPowerPointPercentage);
    Assert.Equal(instance.RestoreAllMoves, properties.RestoreAllMoves);
  }

  [Fact(DisplayName = "ctor: it should construct an instance from arguments.")]
  public void Given_ValidArguments_When_ctor_Then_CorrectProperties()
  {
    bool isHerbal = _faker.Random.Bool();
    bool isHealingPercentage = _faker.Random.Bool();
    bool revives = _faker.Random.Bool();
    int healing = isHealingPercentage
      ? _faker.Random.Int(1, 100)
      : revives
        ? _faker.Random.Int(1, 200)
        : _faker.Random.Int(0, 200);

    StatusCondition? statusCondition = _faker.PickRandom<StatusCondition>();
    bool allConditions = false;

    bool isPowerPointPercentage = _faker.Random.Bool();
    bool restoreAllMoves = _faker.Random.Bool();
    int powerPoints = isPowerPointPercentage
      ? _faker.Random.Int(1, 100)
      : restoreAllMoves
        ? _faker.Random.Int(1, PowerPoints.MaximumValue)
        : _faker.Random.Int(0, PowerPoints.MaximumValue);

    MedicineProperties properties = new(
      isHerbal,
      healing,
      isHealingPercentage,
      revives,
      statusCondition,
      allConditions,
      powerPoints,
      isPowerPointPercentage,
      restoreAllMoves);

    Assert.Equal(isHerbal, properties.IsHerbal);
    Assert.Equal(healing, properties.Healing);
    Assert.Equal(isHealingPercentage, properties.IsHealingPercentage);
    Assert.Equal(revives, properties.Revives);
    Assert.Equal(statusCondition, properties.StatusCondition);
    Assert.Equal(allConditions, properties.AllConditions);
    Assert.Equal(powerPoints, properties.PowerPoints);
    Assert.Equal(isPowerPointPercentage, properties.IsPowerPointPercentage);
    Assert.Equal(restoreAllMoves, properties.RestoreAllMoves);
  }

  [Fact(DisplayName = "ctor: it should construct the default instance.")]
  public void Given_NoArgument_When_ctor_Then_DefaultInstance()
  {
    MedicineProperties properties = new();
    Assert.False(properties.IsHerbal);
    Assert.Equal(0, properties.Healing);
    Assert.False(properties.IsHealingPercentage);
    Assert.False(properties.Revives);
    Assert.Null(properties.StatusCondition);
    Assert.False(properties.AllConditions);
    Assert.Equal(0, properties.PowerPoints);
    Assert.False(properties.IsPowerPointPercentage);
    Assert.False(properties.RestoreAllMoves);
  }

  [Fact(DisplayName = "ctor: it should throw ValidationException when arguments are not valid.")]
  public void Given_InvalidArguments_When_ctor_Then_ValidationException()
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new MedicineProperties(
      isHerbal: false,
      healing: 0,
      isHealingPercentage: true,
      revives: true,
      statusCondition: (StatusCondition)(-1),
      allConditions: true,
      powerPoints: -1,
      isPowerPointPercentage: true,
      restoreAllMoves: true));

    Assert.Equal(6, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Healing");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "Healing");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "StatusCondition");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NullValidator" && e.PropertyName == "StatusCondition");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "PowerPoints");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "PowerPoints");
  }
}
