using Bogus;

namespace PokeGame.Core.Items.Properties;

[Trait(Traits.Category, Categories.Unit)]
public class PokeBallPropertiesTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should construct an instance from another.")]
  public void Given_Instance_When_ctor_Then_CorrectProperties()
  {
    PokeBallProperties instance = new(catchMultiplier: 2.0, heal: true, baseFriendship: 150, friendshipMultiplier: 1.5);
    PokeBallProperties properties = new(instance);
    Assert.Equal(instance.CatchMultiplier, properties.CatchMultiplier);
    Assert.Equal(instance.Heal, properties.Heal);
    Assert.Equal(instance.BaseFriendship, properties.BaseFriendship);
    Assert.Equal(instance.FriendshipMultiplier, properties.FriendshipMultiplier);
  }

  [Fact(DisplayName = "ctor: it should construct an instance from arguments.")]
  public void Given_ValidArguments_When_ctor_Then_CorrectProperties()
  {
    double catchMultiplier = _faker.Random.Int(2, 8) / 2.0;
    bool heal = _faker.Random.Bool();
    byte baseFriendship = (byte)(_faker.Random.Int(0, 20) * 10);
    double friendshipMultiplier = _faker.Random.Int(10, 20) / 10.0;
    PokeBallProperties properties = new(catchMultiplier, heal, baseFriendship, friendshipMultiplier);
    Assert.Equal(catchMultiplier, properties.CatchMultiplier);
    Assert.Equal(heal, properties.Heal);
    Assert.Equal(baseFriendship, properties.BaseFriendship);
    Assert.Equal(friendshipMultiplier, properties.FriendshipMultiplier);
  }

  [Fact(DisplayName = "ctor: it should construct the default instance.")]
  public void Given_NoArgument_When_ctor_Then_DefaultInstance()
  {
    PokeBallProperties properties = new();
    Assert.Equal(1.0, properties.CatchMultiplier);
    Assert.False(properties.Heal);
    Assert.Equal(0, properties.BaseFriendship);
    Assert.Equal(1.0, properties.FriendshipMultiplier);
  }

  [Fact(DisplayName = "ctor: it should throw ValidationException when arguments are not valid.")]
  public void Given_InvalidArguments_When_ctor_Then_ValidationException()
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new PokeBallProperties(0.0, false, 0, -1.5));
    Assert.Equal(2, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "CatchMultiplier");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "FriendshipMultiplier");
  }
}
