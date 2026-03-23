using Bogus;
using PokeGame.Builders;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Moves;

[Trait(Traits.Category, Categories.Unit)]
public class MoveTests
{
  private readonly Faker _faker = new();

  private readonly World _world;

  public MoveTests()
  {
    _world = new WorldBuilder(_faker).Build();
  }

  [Fact(DisplayName = "ctor: it should throw ArgumentOutOfRangeException when the category is not defined.")]
  public void Given_UndefinedCategory_When_ctor_Then_ArgumentOutOfRangeException()
  {
    var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Move(_world, PokemonType.Electric, (MoveCategory)(-1), new Slug("thunder-shock"), new PowerPoints(30)));
    Assert.Equal("category", exception.ParamName);
  }

  [Fact(DisplayName = "ctor: it should throw ArgumentOutOfRangeException when the type is not defined.")]
  public void Given_UndefinedType_When_ctor_Then_ArgumentOutOfRangeException()
  {
    var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Move(_world, (PokemonType)(-1), MoveCategory.Special, new Slug("thunder-shock"), new PowerPoints(30)));
    Assert.Equal("type", exception.ParamName);
  }

  [Fact(DisplayName = "Power: it should throw StatusMoveCannotHavePowerException when the move is Status and power is not null.")]
  public void StatusMove_When_Power_Then_StatusMoveCannotHavePowerException()
  {
    Move move = new(_world, PokemonType.Electric, MoveCategory.Status, new Slug("thunder-wave"), new PowerPoints(20));
    Power power = new(40);
    var exception = Assert.Throws<StatusMoveCannotHavePowerException>(() => move.Power = power);
    Assert.Equal(move.WorldId.ToGuid(), exception.WorldId);
    Assert.Equal(move.EntityId, exception.MoveId);
    Assert.Equal(power, exception.Power);
    Assert.Equal("Power", exception.PropertyName);
  }
}
