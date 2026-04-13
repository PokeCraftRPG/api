using Bogus;
using PokeGame.Builders;
using PokeGame.Core.Moves;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Species;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Varieties;

[Trait(Traits.Category, Categories.Unit)]
public class VarietyTests
{
  private readonly Faker _faker = new();

  private readonly World _world;

  public VarietyTests()
  {
    _world = new WorldBuilder(_faker).Build();
  }

  [Fact(DisplayName = "ctor: it should throw WorldMismatchException when the variety and species are not in the same world.")]
  public void Given_WorldMismatch_When_ctor_Then_WorldMismatchException()
  {
    SpeciesAggregate species = SpeciesBuilder.Eevee();
    VarietyId varietyId = VarietyId.NewId(_world.Id);

    var exception = Assert.Throws<WorldMismatchException>(() => new Variety(species, isDefault: true, species.Key, _world.OwnerId, varietyId));
    Assert.Equal(varietyId.GetEntity(), exception.Expected);
    Assert.Equal(species.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("species", exception.ParamName);
  }

  [Fact(DisplayName = "RemoveMove: it should throw WorldMismatchException when the variety and move are not in the same world.")]
  public void Given_WorldMismatch_When_RemoveMove_Then_WorldMismatchException()
  {
    Variety variety = VarietyBuilder.Pikachu(_faker, _world);
    Move move = MoveBuilder.ThunderShock(_faker);

    var exception = Assert.Throws<WorldMismatchException>(() => variety.RemoveMove(move, _world.OwnerId));
    Assert.Equal(variety.Id.GetEntity(), exception.Expected);
    Assert.Equal(move.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("moveId", exception.ParamName);
  }

  [Fact(DisplayName = "SetEvolutionMove: it should throw WorldMismatchException when the variety and move are not in the same world.")]
  public void Given_WorldMismatch_When_SetEvolutionMove_Then_WorldMismatchException()
  {
    Variety variety = VarietyBuilder.Pikachu(_faker, _world);
    Move move = MoveBuilder.ThunderShock(_faker);

    var exception = Assert.Throws<WorldMismatchException>(() => variety.SetEvolutionMove(move, _world.OwnerId));
    Assert.Equal(variety.Id.GetEntity(), exception.Expected);
    Assert.Equal(move.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("moveId", exception.ParamName);
  }

  [Fact(DisplayName = "SetLevelMove: it should throw WorldMismatchException when the variety and move are not in the same world.")]
  public void Given_WorldMismatch_When_SetLevelMove_Then_WorldMismatchException()
  {
    Variety variety = VarietyBuilder.Pikachu(_faker, _world);
    Move move = MoveBuilder.ThunderShock(_faker);

    var exception = Assert.Throws<WorldMismatchException>(() => variety.SetLevelMove(move, new Level(1), _world.OwnerId));
    Assert.Equal(variety.Id.GetEntity(), exception.Expected);
    Assert.Equal(move.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("moveId", exception.ParamName);
  }
}
