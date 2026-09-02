using Bogus;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Regions;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IRegionBuilder
{
  IRegionBuilder WithId(RegionId regionId);
  IRegionBuilder WithWorld(World? world);
  IRegionBuilder WithKey(string key);
  IRegionBuilder WithName(string? name);
  IRegionBuilder WithSummary(string? summary);
  IRegionBuilder WithContent(string? content);

  Region Build();
}

public class RegionBuilder : IRegionBuilder
{
  private readonly Faker _faker;

  private string? _content;
  private string _key = "kanto";
  private string? _name = "Kanto";
  private RegionId? _regionId;
  private string? _summary;
  private World? _world;

  public RegionBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IRegionBuilder WithId(RegionId regionId)
  {
    _regionId = regionId;
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

  public IRegionBuilder WithSummary(string? summary)
  {
    _summary = summary;
    return this;
  }

  public IRegionBuilder WithContent(string? content)
  {
    _content = content;
    return this;
  }

  public Region Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    ActorId actorId = world.OwnerId.ActorId;
    Key key = new(_key);

    Region region = _regionId.HasValue
      ? new(_regionId.Value, key, actorId)
      : new(world, key, actorId);

    region.Update(Name.TryCreate(_name), Summary.TryCreate(_summary), Content.TryCreate(_content), actorId);

    return region;
  }

  public static Region Kanto(Faker? faker = null, World? world = null) => new RegionBuilder(faker)
    .WithWorld(world)
    .WithKey("kanto")
    .WithName("Kanto")
    .WithSummary("The first region.")
    .WithContent("Home of Pallet Town and Mt. Moon.")
    .Build();

  public static Region Johto(Faker? faker = null, World? world = null) => new RegionBuilder(faker)
    .WithWorld(world)
    .WithKey("johto")
    .WithName("Johto")
    .WithSummary("The second region.")
    .WithContent("Home of Goldenrod City and the Bell Tower.")
    .Build();

  public static Region Hoenn(Faker? faker = null, World? world = null) => new RegionBuilder(faker)
    .WithWorld(world)
    .WithKey("hoenn")
    .WithName("Hoenn")
    .WithSummary("The third region.")
    .WithContent("Home of Littleroot Town and Mt. Chimney.")
    .Build();

  public static Region Sinnoh(Faker? faker = null, World? world = null) => new RegionBuilder(faker)
    .WithWorld(world)
    .WithKey("sinnoh")
    .WithName("Sinnoh")
    .WithSummary("The fourth region.")
    .WithContent("Home of Twinleaf Town and Mt. Coronet.")
    .Build();
}
