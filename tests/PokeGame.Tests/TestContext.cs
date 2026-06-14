using Bogus;
using Krakenar.Contracts;
using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Users;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Actors;
using PokeGame.Core.Identity;
using PokeGame.Core.Worlds;

namespace PokeGame;

public class TestContext : IContext
{
  private readonly Faker _faker;

  public TestContext(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public User? User { get; set; }
  public ActorId? ActorId => User is null ? null : new Actor(User).ToActorId();
  public UserId UserId
  {
    get
    {
      if (User is null)
      {
        throw new InvalidOperationException("An authenticated user is required.");
      }
      return new UserId(User);
    }
  }

  public World? World { get; set; }
  public WorldId WorldId => TryGetWorldId() ?? throw new InvalidOperationException("A world is required.");
  public bool IsWorldOwner => User is not null && World is not null && World.OwnerId == UserId;

  public IReadOnlyCollection<CustomAttribute> GetSessionCustomAttributes()
  {
    List<CustomAttribute> customAttributes = new(capacity: 2);
    customAttributes.Add(new CustomAttribute("AdditionalInformation", $@"{{""User-Agent"":""{_faker.Internet.UserAgent()}""}}"));
    customAttributes.Add(new CustomAttribute("IpAddress", _faker.Internet.Ip()));
    return customAttributes.AsReadOnly();
  }

  public WorldId? TryGetWorldId() => World?.Id;
}
