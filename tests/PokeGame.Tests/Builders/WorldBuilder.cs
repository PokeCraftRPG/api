using Bogus;
using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Users;
using PokeGame.Core;
using PokeGame.Core.Actors;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IWorldBuilder
{
  IWorldBuilder WithId(WorldId? id);
  IWorldBuilder WithUser(User? user);
  IWorldBuilder WithKey(Slug? key);
  IWorldBuilder WithName(Name? name);
  IWorldBuilder WithDescription(Description? description);
  IWorldBuilder ClearChanges(bool clearChanges = true);

  World Build();
}

public class WorldBuilder : IWorldBuilder
{
  private readonly Faker _faker;

  private WorldId? _id = null;
  private User? _user = null;
  private Slug? _key = null;
  private Name? _name = null;
  private Description? _description = null;
  private bool _clearChanges = false;

  public WorldBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IWorldBuilder WithId(WorldId? id)
  {
    _id = id;
    return this;
  }

  public IWorldBuilder WithUser(User? user)
  {
    _user = user;
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

  public IWorldBuilder ClearChanges(bool clearChanges = true)
  {
    _clearChanges = clearChanges;
    return this;
  }

  public World Build()
  {
    User user = _user ?? new UserBuilder(_faker).Build();
    Actor actor = new(user);
    UserId userId = new(actor.GetActorId());
    Slug key = _key ?? new("pokemon-world");

    World world = new(userId, key, _id)
    {
      Name = _name,
      Description = _description
    };
    world.Update(userId);

    if (_clearChanges)
    {
      world.ClearChanges();
    }

    return world;
  }
}
