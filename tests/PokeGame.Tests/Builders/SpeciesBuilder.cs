using Bogus;
using PokeGame.Core;
using PokeGame.Core.Species;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface ISpeciesBuilder
{
  ISpeciesBuilder WithId(SpeciesId? id);
  ISpeciesBuilder WithWorld(World? world);
  ISpeciesBuilder WithCategory(PokemonCategory? category);
  ISpeciesBuilder WithNumber(Number? number);
  ISpeciesBuilder WithKey(Slug? key);
  ISpeciesBuilder WithBaseFriendship(Friendship? baseFriendship);
  ISpeciesBuilder WithCatchRate(CatchRate? catchRate);
  ISpeciesBuilder WithGrowthRate(GrowthRate? growthRate);
  ISpeciesBuilder WithEggCycles(EggCycles? eggCycles);
  ISpeciesBuilder WithEggGroups(EggGroups? eggGroups);
  ISpeciesBuilder ClearChanges(bool clearChanges = true);

  SpeciesAggregate Build();
}

public class SpeciesBuilder : ISpeciesBuilder
{
  private readonly Faker _faker;

  private Friendship? _baseFriendship = null;
  private CatchRate? _catchRate = null;
  private PokemonCategory? _category = null;
  private bool _clearChanges = false;
  private EggCycles? _eggCycles = null;
  private EggGroups? _eggGroups = null;
  private GrowthRate? _growthRate = null;
  private SpeciesId? _id = null;
  private Slug? _key = null;
  private Number? _number = null;
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

  public ISpeciesBuilder WithCategory(PokemonCategory? category)
  {
    _category = category;
    return this;
  }

  public ISpeciesBuilder WithNumber(Number? number)
  {
    _number = number;
    return this;
  }

  public ISpeciesBuilder WithKey(Slug? key)
  {
    _key = key;
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
    EggGroups eggGroups = _eggGroups ?? RandomEggGroups();

    SpeciesAggregate species = _id.HasValue
      ? new(number, category, key, baseFriendship, catchRate, growthRate, eggCycles, eggGroups, world.OwnerId, _id.Value)
      : new(world, number, category, key, baseFriendship, catchRate, growthRate, eggCycles, eggGroups);

    species.Update(world.OwnerId);

    if (_clearChanges)
    {
      species.ClearChanges();
    }

    return species;
  }

  private EggGroups RandomEggGroups()
  {
    EggGroup primary = _faker.PickRandom<EggGroup>();
    EggGroup? secondary = _faker.PickRandom<EggGroup>();
    if (primary == secondary || primary == EggGroup.NoEggsDiscovered || primary == EggGroup.Ditto || secondary == EggGroup.NoEggsDiscovered || secondary == EggGroup.Ditto)
    {
      secondary = null;
    }
    return new EggGroups(primary, secondary);
  }
}
