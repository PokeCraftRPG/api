using Bogus;
using PokeGame.Core.Regions;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IRegionBuilder
{
  IRegionBuilder WithId(Guid id);
  IRegionBuilder WithWorld(World? world);
  IRegionBuilder WithKey(string key);
  IRegionBuilder WithName(string? name);
  IRegionBuilder WithDescription(string? description);

  Region Build();
}

public class RegionBuilder : IRegionBuilder
{
  private readonly Faker _faker;

  private string? _description = null;
  private Guid? _id = null;
  private string _key = "region";
  private string? _name = null;
  private World? _world = null;

  public RegionBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IRegionBuilder WithId(Guid id)
  {
    _id = id;
    return this;
  }

  public IRegionBuilder WithWorld(World? world)
  {
    _world = world;
    return this;
  }

  public IRegionBuilder WithKey(string key)
  {
    _key = key;
    return this;
  }

  public IRegionBuilder WithName(string? name)
  {
    _name = name;
    return this;
  }

  public IRegionBuilder WithDescription(string? description)
  {
    _description = description;
    return this;
  }

  public Region Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    return new Region(world, _key, world.OwnerId, _id, _name, _description);
  }

  public static Region Hoenn(Faker? faker = null, World? world = null) => new RegionBuilder(faker)
    .WithWorld(world)
    .WithKey("hoenn")
    .WithName("Hoenn")
    .Build();
  public static Region Johto(Faker? faker = null, World? world = null) => new RegionBuilder(faker)
    .WithWorld(world)
    .WithKey("johto")
    .WithName("Johto")
    .Build();
  public static Region Kanto(Faker? faker = null, World? world = null) => new RegionBuilder(faker)
    .WithWorld(world)
    .WithKey("kanto")
    .WithName("Kanto")
    .Build();
  public static Region Sinnoh(Faker? faker = null, World? world = null) => new RegionBuilder(faker)
    .WithWorld(world)
    .WithKey("sinnoh")
    .WithName("Sinnoh")
    .Build();
}
