using Bogus;
using PokeGame.Core.Battles;
using PokeGame.Core.Moves;

namespace PokeGame.Core.Items.Properties;

[Trait(Traits.Category, Categories.Unit)]
public class BerryPropertiesTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should construct an instance from another.")]
  public void Given_Instance_When_ctor_Then_CorrectProperties()
  {
    BerryProperties instance = new(
      healing: 30,
      isHealingPercentage: true,
      statusCondition: StatusCondition.Paralysis,
      allConditions: false,
      cureConfusion: false,
      powerPoints: 5,
      attack: 1,
      defense: -2,
      specialAttack: 0,
      specialDefense: 3,
      speed: -1,
      accuracy: 0,
      evasion: 2,
      critical: 1,
      lowerEffortValues: PokemonStatistic.Defense,
      raiseFriendship: true);
    BerryProperties properties = new(instance);
    Assert.Equal(instance.Healing, properties.Healing);
    Assert.Equal(instance.IsHealingPercentage, properties.IsHealingPercentage);
    Assert.Equal(instance.StatusCondition, properties.StatusCondition);
    Assert.Equal(instance.AllConditions, properties.AllConditions);
    Assert.Equal(instance.CureConfusion, properties.CureConfusion);
    Assert.Equal(instance.PowerPoints, properties.PowerPoints);
    Assert.Equal(instance.Attack, properties.Attack);
    Assert.Equal(instance.Defense, properties.Defense);
    Assert.Equal(instance.SpecialAttack, properties.SpecialAttack);
    Assert.Equal(instance.SpecialDefense, properties.SpecialDefense);
    Assert.Equal(instance.Speed, properties.Speed);
    Assert.Equal(instance.Accuracy, properties.Accuracy);
    Assert.Equal(instance.Evasion, properties.Evasion);
    Assert.Equal(instance.Critical, properties.Critical);
    Assert.Equal(instance.LowerEffortValues, properties.LowerEffortValues);
    Assert.Equal(instance.RaiseFriendship, properties.RaiseFriendship);
  }

  [Fact(DisplayName = "ctor: it should construct an instance from arguments.")]
  public void Given_ValidArguments_When_ctor_Then_CorrectProperties()
  {
    bool isHealingPercentage = _faker.Random.Bool();
    int healing = isHealingPercentage ? _faker.Random.Int(1, 100) : _faker.Random.Int(0, 200);
    StatusCondition? statusCondition = _faker.PickRandom<StatusCondition>();
    bool allConditions = false;
    bool cureConfusion = false;

    int powerPoints = _faker.Random.Int(0, PowerPoints.MaximumValue);
    int attack = _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage);
    int defense = _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage);
    int specialAttack = _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage);
    int specialDefense = _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage);
    int speed = _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage);
    int accuracy = _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage);
    int evasion = _faker.Random.Int(StatisticChanges.MinimumStage, StatisticChanges.MaximumStage);
    int critical = _faker.Random.Int(StatisticChanges.MinimumCritical, StatisticChanges.MaximumCritical);
    PokemonStatistic? lowerEffortValues = _faker.Random.Bool() ? _faker.PickRandom<PokemonStatistic>() : null;
    bool raiseFriendship = _faker.Random.Bool();

    BerryProperties properties = new(
      healing,
      isHealingPercentage,
      statusCondition,
      allConditions,
      cureConfusion,
      powerPoints,
      attack,
      defense,
      specialAttack,
      specialDefense,
      speed,
      accuracy,
      evasion,
      critical,
      lowerEffortValues,
      raiseFriendship);

    Assert.Equal(healing, properties.Healing);
    Assert.Equal(isHealingPercentage, properties.IsHealingPercentage);
    Assert.Equal(statusCondition, properties.StatusCondition);
    Assert.Equal(allConditions, properties.AllConditions);
    Assert.Equal(cureConfusion, properties.CureConfusion);
    Assert.Equal(powerPoints, properties.PowerPoints);
    Assert.Equal(attack, properties.Attack);
    Assert.Equal(defense, properties.Defense);
    Assert.Equal(specialAttack, properties.SpecialAttack);
    Assert.Equal(specialDefense, properties.SpecialDefense);
    Assert.Equal(speed, properties.Speed);
    Assert.Equal(accuracy, properties.Accuracy);
    Assert.Equal(evasion, properties.Evasion);
    Assert.Equal(critical, properties.Critical);
    Assert.Equal(lowerEffortValues, properties.LowerEffortValues);
    Assert.Equal(raiseFriendship, properties.RaiseFriendship);
  }

  [Fact(DisplayName = "ctor: it should construct the default instance.")]
  public void Given_NoArgument_When_ctor_Then_DefaultInstance()
  {
    BerryProperties properties = new();
    Assert.Equal(0, properties.Healing);
    Assert.False(properties.IsHealingPercentage);
    Assert.Null(properties.StatusCondition);
    Assert.False(properties.AllConditions);
    Assert.False(properties.CureConfusion);
    Assert.Equal(0, properties.PowerPoints);
    Assert.Equal(0, properties.Attack);
    Assert.Equal(0, properties.Defense);
    Assert.Equal(0, properties.SpecialAttack);
    Assert.Equal(0, properties.SpecialDefense);
    Assert.Equal(0, properties.Speed);
    Assert.Equal(0, properties.Accuracy);
    Assert.Equal(0, properties.Evasion);
    Assert.Equal(0, properties.Critical);
    Assert.Null(properties.LowerEffortValues);
    Assert.False(properties.RaiseFriendship);
  }

  [Fact(DisplayName = "ctor: it should throw ValidationException when arguments are not valid.")]
  public void Given_InvalidArguments_When_ctor_Then_ValidationException()
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new BerryProperties(
      healing: 0,
      isHealingPercentage: true,
      statusCondition: (StatusCondition)(-1),
      allConditions: true,
      cureConfusion: true,
      powerPoints: -1,
      attack: StatisticChanges.MaximumStage + 1,
      defense: 0,
      specialAttack: 0,
      specialDefense: 0,
      speed: 0,
      accuracy: 0,
      evasion: 0,
      critical: 0,
      lowerEffortValues: (PokemonStatistic)(-1),
      raiseFriendship: false));

    Assert.Equal(7, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Healing");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "StatusCondition");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NullValidator" && e.PropertyName == "StatusCondition");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "PowerPoints");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Attack");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "LowerEffortValues");
  }
}
