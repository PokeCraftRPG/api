using Bogus;
using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Users;
using PokeGame.Core;
using PokeGame.Core.Actors;
using PokeGame.Core.Identity;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IWorldBuilder
{
  IWorldBuilder WithId(WorldId worldId);
  IWorldBuilder WithOwner(User? owner);
  IWorldBuilder WithKey(string key);
  IWorldBuilder WithName(string? name);
  IWorldBuilder WithSummary(string? summary);
  IWorldBuilder WithContent(string? content);

  World Build();
}

public class WorldBuilder : IWorldBuilder
{
  private readonly Faker _faker;

  private string? _content = null;
  private string _key = "world";
  private string? _name = "World";
  private User? _owner = null;
  private string? _summary = null;
  private WorldId? _worldId = null;

  public WorldBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IWorldBuilder WithId(WorldId worldId)
  {
    _worldId = worldId;
    return this;
  }

  public IWorldBuilder WithOwner(User? owner)
  {
    _owner = owner;
    return this;
  }

  public IWorldBuilder WithKey(string key)
  {
    _key = key;
    return this;
  }

  public IWorldBuilder WithName(string? name)
  {
    _name = name;
    return this;
  }

  public IWorldBuilder WithSummary(string? summary)
  {
    _summary = summary;
    return this;
  }

  public IWorldBuilder WithContent(string? content)
  {
    _content = content;
    return this;
  }

  public World Build()
  {
    User owner = _owner ?? new UserBuilder(_faker).Build();
    UserId ownerId = new(new Actor(owner).ToActorId());
    Key key = new(_key);

    World world = new(ownerId, key, _worldId);
    world.Update(Name.TryCreate(_name), Summary.TryCreate(_summary), Content.TryCreate(_content), ownerId.ActorId);
    return world;
  }
}
