using Bogus;
using Krakenar.Contracts;
using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Users;
using Logitar;
using Logitar.EventSourcing;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Actors;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;

namespace PokeGame;

public class TestContext : IContext
{
  private readonly Faker _faker;

  public TestContext(Faker? faker = null)
  {
    _faker = faker ?? new();

    User = new UserBuilder(_faker).Build();
    World = new WorldBuilder(_faker).WithUser(User).Build();
  }

  public Actor Actor => User is null ? new() : new(User);
  public ActorId ActorId => Actor.GetActorId();

  public User? User { get; set; }
  public UserId UserId => new(ActorId);

  public World? World { get; set; }
  public WorldId WorldId => World?.Id ?? throw new InvalidOperationException("The world has not been initialized.");
  public Guid WorldUid => WorldId.ToGuid();

  public bool IsWorldOwner => User is not null && World is not null && World.OwnerId == new UserId(new Actor(User).GetActorId());

  public IReadOnlyCollection<CustomAttribute> GetSessionCustomAttributes() =>
  [
    new("AdditionalInformation", $@"{{""User-Agent"":""{_faker.Internet.UserAgent()}""}}"),
    new("IpAddress", _faker.Internet.Ip())
  ];

  public UserId? GetUserId() => Actor.Type == ActorType.User ? new UserId(Actor.GetActorId()) : null;

  public WorldModel? GetWorld()
  {
    if (World is null)
    {
      return null;
    }

    return new WorldModel
    {
      Id = World.Id.ToGuid(),
      Version = World.Version,
      CreatedBy = World.CreatedBy?.ToActor() ?? new Actor(),
      CreatedOn = World.CreatedOn.AsUniversalTime(),
      UpdatedBy = World.UpdatedBy?.ToActor() ?? new Actor(),
      UpdatedOn = World.UpdatedOn.AsUniversalTime(),
      Key = World.Key.Value,
      Name = World.Name?.Value,
      Description = World.Description?.Value,
      Owner = World.OwnerId.ActorId.ToActor()
    };
  }
}
