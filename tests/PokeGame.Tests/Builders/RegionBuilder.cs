using Bogus;
using PokeGame.Core;
using PokeGame.Core.Regions;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IRegionBuilder
{
  IRegionBuilder WithId(RegionId? id);
  IRegionBuilder WithWorld(World? world);
  IRegionBuilder WithKey(Slug? key);
  IRegionBuilder WithName(Name? name);
  IRegionBuilder WithDescription(Description? description);
  IRegionBuilder WithUrl(Url? url);
  IRegionBuilder WithNotes(Notes? notes);
  IRegionBuilder ClearChanges(bool clearChanges = true);

  Region Build();
}

public class RegionBuilder : IRegionBuilder
{
  private readonly Faker _faker;

  private Description? _description = null;
  private RegionId? _id = null;
  private Slug? _key = null;
  private Name? _name = null;
  private Notes? _notes = null;
  private Url? _url = null;
  private World? _world = null;
  private bool _clearChanges = false;

  public RegionBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IRegionBuilder WithId(RegionId? id)
  {
    _id = id;
    return this;
  }

  public IRegionBuilder WithWorld(World? world)
  {
    _world = world;
    return this;
  }

  public IRegionBuilder WithKey(Slug? key)
  {
    _key = key;
    return this;
  }

  public IRegionBuilder WithName(Name? name)
  {
    _name = name;
    return this;
  }

  public IRegionBuilder WithDescription(Description? description)
  {
    _description = description;
    return this;
  }

  public IRegionBuilder WithUrl(Url? url)
  {
    _url = url;
    return this;
  }

  public IRegionBuilder WithNotes(Notes? notes)
  {
    _notes = notes;
    return this;
  }

  public IRegionBuilder ClearChanges(bool clearChanges = true)
  {
    _clearChanges = clearChanges;
    return this;
  }

  public Region Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    Slug key = _key ?? new("a-region");

    Region region = _id.HasValue ? new(key, world.OwnerId, _id.Value) : new(world, key);
    region.Name = _name;
    region.Description = _description;
    region.Url = _url;
    region.Notes = _notes;
    region.Update(world.OwnerId);

    if (_clearChanges)
    {
      region.ClearChanges();
    }

    return region;
  }
}
