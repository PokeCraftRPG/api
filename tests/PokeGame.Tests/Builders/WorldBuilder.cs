using Bogus;
using Krakenar.Contracts.Users;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Identity;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IWorldBuilder
{
  IWorldBuilder WithId(WorldId? worldId);
  IWorldBuilder WithOwner(User? user);
  IWorldBuilder WithKey(Slug? key);
  IWorldBuilder WithName(Name? name);
  IWorldBuilder WithDescription(Description? description);

  World Build();
}

public class WorldBuilder : IWorldBuilder
{
  private readonly Faker _faker = new();

  private Description? _description = null;
  private Slug? _key = null;
  private Name? _name = null;
  private User? _owner = null;
  private WorldId? _worldId = null;

  public WorldBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IWorldBuilder WithId(WorldId? worldId)
  {
    _worldId = worldId;
    return this;
  }

  public IWorldBuilder WithOwner(User? user)
  {
    _owner = user;
    return this;
  }

  public IWorldBuilder WithKey(Slug? key)
  {
    _key = key;
    return this;
  }

  public IWorldBuilder WithName(Name? name)
  {
    _name = name;
    return this;
  }

  public IWorldBuilder WithDescription(Description? description)
  {
    _description = description;
    return this;
  }

  public World Build()
  {
    User owner = _owner ?? new UserBuilder(_faker).Build();
    UserId ownerId = new(owner);
    ActorId actorId = ownerId.ActorId;

    Slug key = _key ?? new("World");

    World world = new(ownerId, key, _worldId);
    world.Rename(_name, actorId);
    world.Describe(_description, actorId);

    return world;
  }
}
