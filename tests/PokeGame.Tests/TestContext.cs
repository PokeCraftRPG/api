using Bogus;
using Krakenar.Contracts;
using Krakenar.Contracts.Users;
using PokeGame.Core;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure;

namespace PokeGame;

public class TestContext : IContext
{
  private readonly Faker _faker;

  public TestContext(Faker faker)
  {
    _faker = faker ?? new();
  }

  public User? User { get; set; }
  public Guid UserId => TryGetUserId() ?? throw new InvalidOperationException("An authenticated user is required.");

  public World? World { get; set; }
  public Guid WorldId => TryGetWorldId() ?? throw new InvalidOperationException("A world is required.");

  public IReadOnlyCollection<CustomAttribute> GetSessionCustomAttributes()
  {
    List<CustomAttribute> customAttributes = new(capacity: 2);
    customAttributes.Add(new CustomAttribute("AdditionalInformation", $@"{{""User-Agent"":""{_faker.Internet.UserAgent()}""}}"));
    customAttributes.Add(new CustomAttribute("IpAddress", _faker.Internet.Ip()));
    return customAttributes.AsReadOnly();
  }

  public bool IsWorldOwner() => User is not null && World is not null && World.OwnerId == User.Id;

  public Guid? TryGetUserId() => User?.Id;
  public Guid? TryGetWorldId() => World?.Id;

  public PokemonContext? Database { get; set; }
  public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    if (Database is null)
    {
      throw new InvalidOperationException("The database context was not initialized.");
    }
    return await Database.SaveChangesAsync(cancellationToken);
  }
}
