using Bogus;
using Krakenar.Contracts.Actors;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Actors;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IWorldBuilder
{
  World Build();
}

public class WorldBuilder : IWorldBuilder
{
  private readonly Faker _faker;

  public WorldBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public World Build()
  {
    Actor actor = new(_faker.Person.FullName)
    {
      RealmId = Guid.NewGuid(),
      Id = Guid.NewGuid(),
      Type = ActorType.User,
      EmailAddress = _faker.Person.Email,
      PictureUrl = _faker.Person.Avatar
    };
    ActorId actorId = actor.GetActorId();
    UserId ownerId = new(actorId);
    Slug key = new("pokemon-world");
    return new World(ownerId, key);
  }
}
