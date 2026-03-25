using Bogus;
using PokeGame.Builders;
using PokeGame.Core.Regions;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Species;

[Trait(Traits.Category, Categories.Unit)]
public class SpeciesAggregateTests
{
  private readonly Faker _faker = new();

  private readonly World _world;

  public SpeciesAggregateTests()
  {
    _world = new WorldBuilder(_faker).Build();
  }

  [Fact(DisplayName = "ctor: it should throw ArgumentOutOfRangeException when the category is not defined.")]
  public void Given_CategoryNotDefined_When_ctor_Then_ArgumentOutOfRangeException()
  {
    var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new SpeciesAggregate(_world, new Number(25), (PokemonCategory)(-1), new Slug("pikachu"),
      new Friendship(70), new CatchRate(190), GrowthRate.MediumFast, new EggCycles(10), new EggGroups(EggGroup.Field, EggGroup.Fairy)));
    Assert.Equal("category", exception.ParamName);
  }

  [Fact(DisplayName = "ctor: it should throw ArgumentOutOfRangeException when the growth rate is not defined.")]
  public void Given_GrowthRateNotDefined_When_ctor_Then_ArgumentOutOfRangeException()
  {
    var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new SpeciesAggregate(_world, new Number(25), PokemonCategory.Standard, new Slug("pikachu"),
      new Friendship(70), new CatchRate(190), (GrowthRate)99, new EggCycles(10), new EggGroups(EggGroup.Field, EggGroup.Fairy)));
    Assert.Equal("growthRate", exception.ParamName);
  }

  [Fact(DisplayName = "SetRegionalNumber: it should throw WorldMismatchException when the region and the species are not in the same world.")]
  public void Given_DifferentWorlds_When_SetRegionalNumber_Then_WorldMismatchException()
  {
    SpeciesAggregate species = new SpeciesBuilder(_faker).WithWorld(_world).Build();
    Region region = new RegionBuilder().Build();

    Assert.True(species.CreatedBy.HasValue);
    var exception = Assert.Throws<WorldMismatchException>(() => species.SetRegionalNumber(region, new Number(25), new UserId(species.CreatedBy.Value)));
    Assert.Equal(species.GetEntity(), exception.Expected);
    Assert.Equal(region.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("regionId", exception.ParamName);
  }
}
