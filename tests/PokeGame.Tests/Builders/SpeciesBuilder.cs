using Bogus;
using PokeGame.Core;
using PokeGame.Core.Regions;
using PokeGame.Core.Species;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface ISpeciesBuilder
{
  ISpeciesBuilder WithId(SpeciesId? id);
  ISpeciesBuilder WithWorld(World? world);
  ISpeciesBuilder WithNumber(Number? number);
  ISpeciesBuilder WithCategory(PokemonCategory? category);
  ISpeciesBuilder WithKey(Slug? key);
  ISpeciesBuilder WithName(Name? name);
  ISpeciesBuilder WithBaseFriendship(Friendship? baseFriendship);
  ISpeciesBuilder WithCatchRate(CatchRate? catchRate);
  ISpeciesBuilder WithGrowthRate(GrowthRate? growthRate);
  ISpeciesBuilder WithEggCycles(EggCycles? eggCycles);
  ISpeciesBuilder WithEggGroups(EggGroups? eggGroups);
  ISpeciesBuilder WithUrl(Url? url);
  ISpeciesBuilder WithNotes(Notes? notes);
  ISpeciesBuilder WithRegionalNumber(Region region, Number? number);
  ISpeciesBuilder WithRegionalNumber(RegionId regionId, Number? number);
  ISpeciesBuilder ClearChanges(bool clearChanges = true);

  SpeciesAggregate Build();
}

public class SpeciesBuilder : ISpeciesBuilder
{
  private readonly Faker _faker;
  private readonly Dictionary<RegionId, Number?> _regionalNumbers = [];

  private Friendship? _baseFriendship = null;
  private CatchRate? _catchRate = null;
  private PokemonCategory? _category = null;
  private bool _clearChanges = false;
  private EggCycles? _eggCycles = null;
  private EggGroups? _eggGroups = null;
  private GrowthRate? _growthRate = null;
  private SpeciesId? _id = null;
  private Slug? _key = null;
  private Name? _name = null;
  private Notes? _notes = null;
  private Number? _number = null;
  private Url? _url = null;
  private World? _world = null;

  public SpeciesBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public ISpeciesBuilder WithId(SpeciesId? id)
  {
    _id = id;
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

  public ISpeciesBuilder WithCategory(PokemonCategory? category)
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

  public ISpeciesBuilder WithGrowthRate(GrowthRate? growthRate)
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

  public ISpeciesBuilder WithUrl(Url? url)
  {
    _url = url;
    return this;
  }

  public ISpeciesBuilder WithNotes(Notes? notes)
  {
    _notes = notes;
    return this;
  }

  public ISpeciesBuilder WithRegionalNumber(Region region, Number? number) => WithRegionalNumber(region.Id, number);
  public ISpeciesBuilder WithRegionalNumber(RegionId regionId, Number? number)
  {
    _regionalNumbers[regionId] = number;
    return this;
  }

  public ISpeciesBuilder ClearChanges(bool clearChanges = true)
  {
    _clearChanges = clearChanges;
    return this;
  }

  public SpeciesAggregate Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    PokemonCategory category = _category ?? PokemonCategory.Standard;
    Number number = _number ?? new(_faker.Random.Int(1, 9999));
    Slug key = _key ?? new("a-species");
    Friendship baseFriendship = _baseFriendship ?? new(_faker.Random.Byte());
    CatchRate catchRate = _catchRate ?? new(_faker.Random.Byte(min: 1));
    GrowthRate growthRate = _growthRate ?? _faker.PickRandom<GrowthRate>();
    EggCycles eggCycles = _eggCycles ?? new(_faker.Random.Byte(min: 1));
    EggGroups eggGroups = _eggGroups ?? _faker.EggGroups();

    SpeciesAggregate species = _id.HasValue
      ? new(number, category, key, baseFriendship, catchRate, growthRate, eggCycles, eggGroups, world.OwnerId, _id.Value)
      : new(world, number, category, key, baseFriendship, catchRate, growthRate, eggCycles, eggGroups);

    species.Name = _name;

    species.Update(world.OwnerId);

    foreach (KeyValuePair<RegionId, Number?> regionalNumber in _regionalNumbers)
    {
      species.SetRegionalNumber(regionalNumber.Key, regionalNumber.Value, world.OwnerId);
    }

    if (_clearChanges)
    {
      species.ClearChanges();
    }

    return species;
  }

  public static SpeciesAggregate Eevee(Faker? faker = null, World? world = null) => new SpeciesBuilder(faker)
    .WithWorld(world)
    .WithNumber(new Number(133))
    .WithCategory(PokemonCategory.Standard)
    .WithKey(new Slug("eevee"))
    .WithName(new Name("Eevee"))
    .WithBaseFriendship(new Friendship(70))
    .WithCatchRate(new CatchRate(45))
    .WithGrowthRate(GrowthRate.MediumFast)
    .WithEggCycles(new EggCycles(35))
    .WithEggGroups(new EggGroups(EggGroup.Field))
    .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Eevee_(Pok%C3%A9mon)"))
    .WithNotes(new Notes("Eevee is a “blank slate” Pokémon designed for multiple evolutions (8 total), with unique mechanics, cultural impact, and evolution methods across games and media."))
    .Build();

  public static SpeciesAggregate Pichu(Faker? faker = null, World? world = null) => new SpeciesBuilder(faker)
    .WithWorld(world)
    .WithNumber(new Number(172))
    .WithCategory(PokemonCategory.Baby)
    .WithKey(new Slug("pichu"))
    .WithName(new Name("Pichu"))
    .WithBaseFriendship(new Friendship(70))
    .WithCatchRate(new CatchRate(190))
    .WithGrowthRate(GrowthRate.MediumFast)
    .WithEggCycles(new EggCycles(10))
    .WithEggGroups(new EggGroups(EggGroup.NoEggsDiscovered))
    .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Pichu_(Pok%C3%A9mon)"))
    .WithNotes(new Notes("Pichu: weakest Electric-type stats; Pikachu’s pre-evolution. Designed as its “next” form, inspired by rodents; notable trivia and naming origins included."))
    .Build();

  public static SpeciesAggregate Pikachu(Faker? faker = null, World? world = null) => new SpeciesBuilder(faker)
    .WithWorld(world)
    .WithNumber(new Number(25))
    .WithCategory(PokemonCategory.Standard)
    .WithKey(new Slug("pikachu"))
    .WithName(new Name("Pikachu"))
    .WithBaseFriendship(new Friendship(70))
    .WithCatchRate(new CatchRate(190))
    .WithGrowthRate(GrowthRate.MediumFast)
    .WithEggCycles(new EggCycles(10))
    .WithEggGroups(new EggGroups(EggGroup.Field, EggGroup.Fairy))
    .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Pikachu_(Pok%C3%A9mon)"))
    .WithNotes(new Notes("Iconic Pokémon: Pikachu has many exclusives (Z-Moves, events), a unique starter role, varied cries/designs, and major cultural and scientific impact."))
    .Build();

  public static SpeciesAggregate Raichu(Faker? faker = null, World? world = null) => new SpeciesBuilder(faker)
    .WithWorld(world)
    .WithNumber(new Number(26))
    .WithCategory(PokemonCategory.Standard)
    .WithKey(new Slug("raichu"))
    .WithName(new Name("Raichu"))
    .WithBaseFriendship(new Friendship(70))
    .WithCatchRate(new CatchRate(75))
    .WithGrowthRate(GrowthRate.MediumFast)
    .WithEggCycles(new EggCycles(10))
    .WithEggGroups(new EggGroups(EggGroup.Field, EggGroup.Fairy))
    .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Raichu_(Pok%C3%A9mon)"))
    .WithNotes(new Notes("Raichu trivia: Mouse Pokémon; can discharge up to 100,000 volts; notable forms (Alolan, Mega) and unique traits across games and lore."))
    .Build();
}
