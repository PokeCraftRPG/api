using Bogus;
using PokeGame.Core.Species;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface ISpeciesBuilder
{
  ISpeciesBuilder WithId(Guid id);
  ISpeciesBuilder WithWorld(World? world);
  ISpeciesBuilder WithNumber(int number);
  ISpeciesBuilder WithCategory(PokemonCategory category);
  ISpeciesBuilder WithKey(string key);
  ISpeciesBuilder WithName(string? name);
  ISpeciesBuilder WithDescription(string? description);
  ISpeciesBuilder WithBaseFriendship(int baseFriendship);
  ISpeciesBuilder WithCatchRate(int catchRate);
  ISpeciesBuilder WithGrowthRate(GrowthRate growthRate);
  ISpeciesBuilder WithEggCycles(int eggCycles);
  ISpeciesBuilder WithEggGroups(EggGroup primary, EggGroup? secondary = null);

  PokemonSpecies Build();
}

public class SpeciesBuilder : ISpeciesBuilder
{
  private readonly Faker _faker;

  private int _baseFriendship = 50;
  private int _catchRate = 255;
  private PokemonCategory _category = PokemonCategory.Standard;
  private string? _description = null;
  private int _eggCycles = 35;
  private GrowthRate _growthRate = GrowthRate.MediumFast;
  private Guid? _id = null;
  private string _key = "species";
  private string? _name = null;
  private int? _number = null;
  private EggGroup _primaryEggGroup = EggGroup.NoEggsDiscovered;
  private EggGroup? _secondaryEggGroup = null;
  private World? _world = null;

  public SpeciesBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public ISpeciesBuilder WithId(Guid id)
  {
    _id = id;
    return this;
  }

  public ISpeciesBuilder WithWorld(World? world)
  {
    _world = world;
    return this;
  }

  public ISpeciesBuilder WithNumber(int number)
  {
    _number = number;
    return this;
  }

  public ISpeciesBuilder WithCategory(PokemonCategory category)
  {
    _category = category;
    return this;
  }

  public ISpeciesBuilder WithKey(string key)
  {
    _key = key;
    return this;
  }

  public ISpeciesBuilder WithName(string? name)
  {
    _name = name;
    return this;
  }

  public ISpeciesBuilder WithDescription(string? description)
  {
    _description = description;
    return this;
  }

  public ISpeciesBuilder WithBaseFriendship(int baseFriendship)
  {
    _baseFriendship = baseFriendship;
    return this;
  }

  public ISpeciesBuilder WithCatchRate(int catchRate)
  {
    _catchRate = catchRate;
    return this;
  }

  public ISpeciesBuilder WithGrowthRate(GrowthRate growthRate)
  {
    _growthRate = growthRate;
    return this;
  }

  public ISpeciesBuilder WithEggCycles(int eggCycles)
  {
    _eggCycles = eggCycles;
    return this;
  }

  public ISpeciesBuilder WithEggGroups(EggGroup primary, EggGroup? secondary = null)
  {
    _primaryEggGroup = primary;
    _secondaryEggGroup = secondary;
    return this;
  }

  public PokemonSpecies Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    return new PokemonSpecies(
      world,
      _number ?? _faker.Random.Int(1, 9999),
      _key,
      _catchRate,
      _eggCycles,
      world.OwnerId,
      _id,
      _category,
      _name,
      _description,
      _baseFriendship,
      _growthRate,
      _primaryEggGroup,
      _secondaryEggGroup);
  }

  public static PokemonSpecies Eevee(Faker? faker = null, World? world = null) => new SpeciesBuilder(faker)
    .WithWorld(world)
    .WithNumber(133)
    .WithCategory(PokemonCategory.Standard)
    .WithKey("eevee")
    .WithName("Eevee")
    .WithBaseFriendship(50)
    .WithCatchRate(45)
    .WithGrowthRate(GrowthRate.MediumFast)
    .WithEggCycles(35)
    .WithEggGroups(EggGroup.Field)
    .Build();
  public static PokemonSpecies Pichu(Faker? faker = null, World? world = null) => new SpeciesBuilder(faker)
    .WithWorld(world)
    .WithNumber(172)
    .WithCategory(PokemonCategory.Baby)
    .WithKey("pichu")
    .WithName("Pichu")
    .WithBaseFriendship(50)
    .WithCatchRate(190)
    .WithGrowthRate(GrowthRate.MediumFast)
    .WithEggCycles(10)
    .WithEggGroups(EggGroup.NoEggsDiscovered)
    .Build();
  public static PokemonSpecies Pikachu(Faker? faker = null, World? world = null) => new SpeciesBuilder(faker)
    .WithWorld(world)
    .WithNumber(25)
    .WithCategory(PokemonCategory.Standard)
    .WithKey("pikachu")
    .WithName("Pikachu")
    .WithBaseFriendship(50)
    .WithCatchRate(190)
    .WithGrowthRate(GrowthRate.MediumFast)
    .WithEggCycles(10)
    .WithEggGroups(EggGroup.Field, EggGroup.Fairy)
    .Build();
  public static PokemonSpecies Raichu(Faker? faker = null, World? world = null) => new SpeciesBuilder(faker)
    .WithWorld(world)
    .WithNumber(26)
    .WithCategory(PokemonCategory.Standard)
    .WithKey("raichu")
    .WithName("Raichu")
    .WithBaseFriendship(50)
    .WithCatchRate(75)
    .WithGrowthRate(GrowthRate.MediumFast)
    .WithEggCycles(10)
    .WithEggGroups(EggGroup.Field, EggGroup.Fairy)
    .Build();
}
