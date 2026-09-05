using Bogus;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Species;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface ISpeciesBuilder
{
  ISpeciesBuilder WithId(SpeciesId speciesId);
  ISpeciesBuilder WithWorld(World? world);
  ISpeciesBuilder WithNumber(int number);
  ISpeciesBuilder WithCategory(SpeciesCategory category);
  ISpeciesBuilder WithKey(string key);
  ISpeciesBuilder WithName(string? name);
  ISpeciesBuilder WithSummary(string? summary);
  ISpeciesBuilder WithContent(string? content);
  ISpeciesBuilder WithBaseFriendship(int baseFriendship);
  ISpeciesBuilder WithCatchRate(int catchRate);
  ISpeciesBuilder WithGrowthRate(GrowthRate growthRate);
  ISpeciesBuilder WithEggs(int cycles, EggGroup primaryGroup, EggGroup? secondaryGroup = null);

  PokemonSpecies Build();
}

public class SpeciesBuilder : ISpeciesBuilder
{
  private readonly Faker _faker;

  private int _baseFriendship = 70;
  private int _catchRate = 45;
  private SpeciesCategory _category = SpeciesCategory.Standard;
  private string? _content;
  private int _eggCycles = 20;
  private EggGroup _primaryEggGroup = EggGroup.Monster;
  private EggGroup? _secondaryEggGroup = EggGroup.Grass;
  private GrowthRate _growthRate = GrowthRate.MediumSlow;
  private string _key = "bulbasaur";
  private string? _name = "Bulbasaur";
  private int _number = 1;
  private SpeciesId? _speciesId;
  private string? _summary;
  private World? _world;

  public SpeciesBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public ISpeciesBuilder WithId(SpeciesId speciesId)
  {
    _speciesId = speciesId;
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

  public ISpeciesBuilder WithCategory(SpeciesCategory category)
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

  public ISpeciesBuilder WithSummary(string? summary)
  {
    _summary = summary;
    return this;
  }

  public ISpeciesBuilder WithContent(string? content)
  {
    _content = content;
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

  public ISpeciesBuilder WithEggs(int cycles, EggGroup primaryGroup, EggGroup? secondaryGroup = null)
  {
    _eggCycles = cycles;
    _primaryEggGroup = primaryGroup;
    _secondaryEggGroup = secondaryGroup;
    return this;
  }

  public PokemonSpecies Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    ActorId actorId = world.OwnerId.ActorId;
    Key key = new(_key);
    Number number = new(_number);
    Friendship baseFriendship = new(_baseFriendship);
    CatchRate catchRate = new(_catchRate);
    SpeciesEggs eggs = new(_eggCycles, _primaryEggGroup, _secondaryEggGroup);

    PokemonSpecies species = _speciesId.HasValue
      ? new(_speciesId.Value, number, _category, key, baseFriendship, catchRate, _growthRate, eggs, actorId)
      : new(world, number, _category, key, baseFriendship, catchRate, _growthRate, eggs, actorId);

    species.SetDetails(Name.TryCreate(_name), Summary.TryCreate(_summary), Content.TryCreate(_content), actorId);

    return species;
  }

  public static PokemonSpecies Bulbasaur(Faker? faker = null, World? world = null) => new SpeciesBuilder(faker)
    .WithWorld(world)
    .WithNumber(1)
    .WithKey("bulbasaur")
    .WithName("Bulbasaur")
    .WithSummary("A seed Pokémon.")
    .WithContent("A strange seed was planted on its back at birth.")
    .WithBaseFriendship(70)
    .WithCatchRate(45)
    .WithGrowthRate(GrowthRate.MediumSlow)
    .WithEggs(20, EggGroup.Monster, EggGroup.Grass)
    .Build();

  public static PokemonSpecies Charmander(Faker? faker = null, World? world = null) => new SpeciesBuilder(faker)
    .WithWorld(world)
    .WithNumber(4)
    .WithKey("charmander")
    .WithName("Charmander")
    .WithSummary("A lizard Pokémon.")
    .WithContent("It has a preference for hot things.")
    .WithBaseFriendship(70)
    .WithCatchRate(45)
    .WithGrowthRate(GrowthRate.MediumSlow)
    .WithEggs(20, EggGroup.Monster, EggGroup.Dragon)
    .Build();

  public static PokemonSpecies Squirtle(Faker? faker = null, World? world = null) => new SpeciesBuilder(faker)
    .WithWorld(world)
    .WithNumber(7)
    .WithKey("squirtle")
    .WithName("Squirtle")
    .WithSummary("A tiny turtle Pokémon.")
    .WithContent("When it retracts its long neck into its shell, it squirts water.")
    .WithBaseFriendship(70)
    .WithCatchRate(45)
    .WithGrowthRate(GrowthRate.MediumSlow)
    .WithEggs(20, EggGroup.Monster, EggGroup.Water1)
    .Build();

  public static PokemonSpecies Pikachu(Faker? faker = null, World? world = null) => new SpeciesBuilder(faker)
    .WithWorld(world)
    .WithNumber(25)
    .WithKey("pikachu")
    .WithName("Pikachu")
    .WithSummary("A mouse Pokémon.")
    .WithContent("When several of these Pokémon gather, their electricity can cause lightning storms.")
    .WithBaseFriendship(50)
    .WithCatchRate(190)
    .WithGrowthRate(GrowthRate.MediumFast)
    .WithEggs(10, EggGroup.Field, EggGroup.Fairy)
    .Build();
}
