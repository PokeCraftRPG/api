using Bogus;
using PokeGame.Builders;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Trainers;

[Trait(Traits.Category, Categories.Unit)]
public class TrainerTests
{
  private readonly Faker _faker = new();

  private readonly World _world;

  public TrainerTests()
  {
    _world = new WorldBuilder(_faker).Build();
  }

  [Fact(DisplayName = "ctor: it should throw ArgumentOutOfRangeException when the gender is not valid.")]
  public void Given_InvalidGender_When_ctor_Then_ArgumentOutOfRangeException()
  {
    License license = new("Q-123456-3");
    Slug key = new("ash-ketchum");
    var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Trainer(_world, license, key, (TrainerGender)(-1)));
    Assert.Equal("gender", exception.ParamName);
  }

  [Fact(DisplayName = "Gender: it should throw ArgumentOutOfRangeException when the gender is not valid.")]
  public void Given_InvalidGender_When_Gender_Then_ArgumentOutOfRangeException()
  {
    Trainer trainer = new(_world, new License("Q-123456-3"), new Slug("ash-ketchum"), TrainerGender.Male);
    var exception = Assert.Throws<ArgumentOutOfRangeException>(() => trainer.Gender = (TrainerGender)(-1));
    Assert.Equal("Gender", exception.ParamName);
  }
}
