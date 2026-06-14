using Bogus;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Regions;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IRegionBuilder
{
  IRegionBuilder WithId(RegionId? regionId);
  IRegionBuilder WithWorld(World? world);
  IRegionBuilder WithKey(Slug? key);
  IRegionBuilder WithName(Name? name);
  IRegionBuilder WithDescription(Description? description);

  Region Build();
}

public class RegionBuilder : IRegionBuilder
{
  private readonly Faker _faker = new();

  private Description? _description = null;
  private Slug? _key = null;
  private Name? _name = null;
  private RegionId? _regionId = null;
  private World? _world = null;

  public RegionBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IRegionBuilder WithId(RegionId? regionId)
  {
    _regionId = regionId;
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

  public Region Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    Slug key = _key ?? new("Region");
    ActorId actorId = world.OwnerId.ActorId;

    Region region = _regionId.HasValue ? new(_regionId.Value, key, actorId) : new(world, key, actorId);
    region.Rename(_name, actorId);
    region.Describe(_description, actorId);

    return region;
  }

  public static Region Hoenn(Faker? faker = null, World? world = null) => new RegionBuilder(faker)
    .WithWorld(world)
    .WithKey(new Slug("hoenn"))
    .WithName(new Name("Hoenn"))
    .Build();
  public static Region Johto(Faker? faker = null, World? world = null) => new RegionBuilder(faker)
    .WithWorld(world)
    .WithKey(new Slug("johto"))
    .WithName(new Name("Johto"))
    .Build();
  public static Region Kanto(Faker? faker = null, World? world = null) => new RegionBuilder(faker)
    .WithWorld(world)
    .WithKey(new Slug("kanto"))
    .WithName(new Name("Kanto"))
    .Build();
  public static Region Sinnoh(Faker? faker = null, World? world = null) => new RegionBuilder(faker)
    .WithWorld(world)
    .WithKey(new Slug("sinnoh"))
    .WithName(new Name("Sinnoh"))
    .Build();
}
