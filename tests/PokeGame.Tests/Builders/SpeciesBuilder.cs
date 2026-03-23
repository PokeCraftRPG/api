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
  ISpeciesBuilder WithKey(Slug? key);
  ISpeciesBuilder WithName(Name? name);
  ISpeciesBuilder ClearChanges(bool clearChanges = true);

  SpeciesAggregate Build();
}

public class SpeciesBuilder : ISpeciesBuilder
{
  private readonly Faker _faker;

  private PokemonCategory? _category = null;
  private bool _clearChanges = false;
  private SpeciesId? _id = null;
  private Slug? _key = null;
  private Name? _name = null;
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

  public ISpeciesBuilder ClearChanges(bool clearChanges = true)
  {
    _clearChanges = clearChanges;
    return this;
  }

  public SpeciesAggregate Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    PokemonCategory category = _category ?? PokemonCategory.Standard;
    Slug key = _key ?? new("a-species");

    SpeciesAggregate species = _id.HasValue ? new(category, key, world.OwnerId, _id.Value) : new(world, category, key);
    species.Name = _name;
    species.Update(world.OwnerId);

    if (_clearChanges)
    {
      species.ClearChanges();
    }

    return species;
  }
}
