using Bogus;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Regions;
using PokeGame.Core.Species;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface ISpeciesBuilder
{
  ISpeciesBuilder WithId(SpeciesId? speciesId);
  ISpeciesBuilder WithWorld(World? world);
  ISpeciesBuilder WithNumber(Number? number);
  ISpeciesBuilder WithCategory(PokemonCategory category);
  ISpeciesBuilder WithKey(Slug? key);
  ISpeciesBuilder WithName(Name? name);
  ISpeciesBuilder WithDescription(Description? description);
  ISpeciesBuilder WithBaseFriendship(Friendship? baseFriendship);
  ISpeciesBuilder WithCatchRate(CatchRate? catchRate);
  ISpeciesBuilder WithGrowthRate(GrowthRate growthRate);
  ISpeciesBuilder WithEggCycles(EggCycles? eggCycles);
  ISpeciesBuilder WithEggGroups(EggGroups? eggGroups);
  ISpeciesBuilder WithRegionalNumber(RegionId regionId, Number number);

  PokemonSpecies Build();
}

public class SpeciesBuilder : ISpeciesBuilder
{
  private readonly Faker _faker = new();

  private Friendship? _baseFriendship = null;
  private PokemonCategory _category = PokemonCategory.Standard;
  private CatchRate? _catchRate = null;
  private Description? _description = null;
  private EggCycles? _eggCycles = null;
  private EggGroups? _eggGroups = null;
  private GrowthRate _growthRate = GrowthRate.MediumFast;
  private Slug? _key = null;
  private Name? _name = null;
  private Number? _number = null;
  private readonly Dictionary<RegionId, Number> _regionalNumbers = [];
  private SpeciesId? _speciesId = null;
  private World? _world = null;

  public SpeciesBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public ISpeciesBuilder WithId(SpeciesId? speciesId)
  {
    _speciesId = speciesId;
    return this;
  }

  public ISpeciesBuilder WithWorld(World? world)
  {
    _world = world;
    return this;
  }

  public ISpeciesBuilder WithNumber(Number? number)
  {
    _number = number;
    return this;
  }

  public ISpeciesBuilder WithCategory(PokemonCategory category)
  {
    _category = category;
    return this;
  }

  public ISpeciesBuilder WithKey(Slug? key)
  {
    _key = key;
    return this;
  }

  public ISpeciesBuilder WithName(Name? name)
  {
    _name = name;
    return this;
  }

  public ISpeciesBuilder WithDescription(Description? description)
  {
    _description = description;
    return this;
  }

  public ISpeciesBuilder WithBaseFriendship(Friendship? baseFriendship)
  {
    _baseFriendship = baseFriendship;
    return this;
  }

  public ISpeciesBuilder WithCatchRate(CatchRate? catchRate)
  {
    _catchRate = catchRate;
    return this;
  }

  public ISpeciesBuilder WithGrowthRate(GrowthRate growthRate)
  {
    _growthRate = growthRate;
    return this;
  }

  public ISpeciesBuilder WithEggCycles(EggCycles? eggCycles)
  {
    _eggCycles = eggCycles;
    return this;
  }

  public ISpeciesBuilder WithEggGroups(EggGroups? eggGroups)
  {
    _eggGroups = eggGroups;
    return this;
  }

  public ISpeciesBuilder WithRegionalNumber(RegionId regionId, Number number)
  {
    _regionalNumbers[regionId] = number;
    return this;
  }

  public PokemonSpecies Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    Number number = _number ?? new Number(1);
    Slug key = _key ?? new Slug("species");
    CatchRate catchRate = _catchRate ?? new CatchRate(45);
    EggCycles eggCycles = _eggCycles ?? new EggCycles(20);
    ActorId actorId = world.OwnerId.ActorId;

    PokemonSpecies species = _speciesId.HasValue
      ? new(_speciesId.Value, number, _category, key, catchRate, eggCycles, _baseFriendship, _growthRate, _eggGroups, actorId)
      : new(world, number, _category, key, catchRate, eggCycles, _baseFriendship, _growthRate, _eggGroups, actorId);
    species.Rename(_name, actorId);
    species.Describe(_description, actorId);

    foreach (KeyValuePair<RegionId, Number> regionalNumber in _regionalNumbers)
    {
      species.SetRegionalNumber(regionalNumber.Key, regionalNumber.Value, actorId);
    }

    return species;
  }

  public static PokemonSpecies Bulbasaur(Faker? faker = null, World? world = null) => new SpeciesBuilder(faker)
    .WithWorld(world)
    .WithNumber(new Number(1))
    .WithCategory(PokemonCategory.Standard)
    .WithKey(new Slug("bulbasaur"))
    .WithName(new Name("Bulbasaur"))
    .WithDescription(new Description("While it is young, it uses the nutrients stored in the seed on its back in order to grow."))
    .WithBaseFriendship(new Friendship(70))
    .WithCatchRate(new CatchRate(45))
    .WithGrowthRate(GrowthRate.MediumSlow)
    .WithEggCycles(new EggCycles(20))
    .WithEggGroups(new EggGroups(EggGroup.Monster, EggGroup.Grass))
    .Build();
  public static PokemonSpecies Charmander(Faker? faker = null, World? world = null) => new SpeciesBuilder(faker)
    .WithWorld(world)
    .WithNumber(new Number(4))
    .WithCategory(PokemonCategory.Standard)
    .WithKey(new Slug("charmander"))
    .WithName(new Name("Charmander"))
    .WithDescription(new Description("The flame on its tail indicates Charmander's life force. If it is healthy, the flame burns intensely."))
    .WithBaseFriendship(new Friendship(70))
    .WithCatchRate(new CatchRate(45))
    .WithGrowthRate(GrowthRate.MediumSlow)
    .WithEggCycles(new EggCycles(20))
    .WithEggGroups(new EggGroups(EggGroup.Monster, EggGroup.Dragon))
    .Build();
  public static PokemonSpecies Pikachu(Faker? faker = null, World? world = null) => new SpeciesBuilder(faker)
    .WithWorld(world)
    .WithNumber(new Number(25))
    .WithCategory(PokemonCategory.Standard)
    .WithKey(new Slug("pikachu"))
    .WithName(new Name("Pikachu"))
    .WithDescription(new Description("When several of these Pokémon gather, their electricity can build and cause lightning storms."))
    .WithBaseFriendship(new Friendship(70))
    .WithCatchRate(new CatchRate(190))
    .WithGrowthRate(GrowthRate.MediumFast)
    .WithEggCycles(new EggCycles(10))
    .WithEggGroups(new EggGroups(EggGroup.Field, EggGroup.Fairy))
    .Build();
}
