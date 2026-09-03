using Bogus;
using Krakenar.Client.Users;
using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Krakenar.Contracts.Users;
using Logitar;
using Logitar.CQRS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure;

namespace PokeGame;

public abstract class IntegrationTests : IAsyncLifetime
{
  private readonly Actor _system = new();

  protected virtual Faker Faker { get; set; }
  protected virtual TestContext Context { get; set; }

  protected virtual IConfiguration Configuration { get; set; }
  protected virtual IServiceProvider ServiceProvider { get; set; }

  protected virtual Actor Actor => Context.User is null ? _system : new(Context.User);
  protected virtual Mock<IUserClient> UserClient { get; set; } = new();

  protected IntegrationTests()
  {
    Faker = new();
    Context = new(Faker);

    Configuration = BuildConfiguration();
    ServiceProvider = BuildServiceProvider();
  }

  protected virtual IConfiguration BuildConfiguration() => new ConfigurationBuilder()
      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
      .Build();

  protected virtual IServiceProvider BuildServiceProvider()
  {
    string? connectionString = EnvironmentHelper.TryGetString("POSTGRESQLCONNSTR_Pokemon")
      ?? Configuration.GetConnectionString("PostgreSQL")
      ?? throw new InvalidOperationException("The PostgreSQL connection string was not found.");

    ServiceCollection services = new();
    services.AddSingleton(Configuration);

    services.AddPokeGameCore();
    services.AddPokeGameInfrastructure(connectionString.Replace("{Database}", GetType().Name));
    services.AddSingleton<IContext>(Context);
    services.AddSingleton(UserClient.Object);

    return services.BuildServiceProvider();
  }

  public virtual async Task InitializeAsync()
  {
    await MigrateDatabaseAsync();
    await ClearDatabaseAsync();
    await InitializeDatabaseAsync();
  }
  protected virtual async Task MigrateDatabaseAsync()
  {
    ICommandBus commandBus = ServiceProvider.GetRequiredService<ICommandBus>();
    await commandBus.ExecuteAsync(new MigrateDatabaseCommand());
  }
  protected virtual async Task ClearDatabaseAsync()
  {
    PokemonContext pokemon = ServiceProvider.GetRequiredService<PokemonContext>();
    StringBuilder sql = new();
    sql.AppendLine(@"DELETE FROM ""Pokemon"".""Regions"";");
    sql.AppendLine(@"DELETE FROM ""Pokemon"".""Moves"";");
    sql.AppendLine(@"DELETE FROM ""Pokemon"".""Abilities"";");
    sql.AppendLine(@"DELETE FROM ""Pokemon"".""Worlds"";");
    sql.AppendLine(@"DELETE FROM ""EventSourcing"".""Events"";");
    sql.AppendLine(@"DELETE FROM ""EventSourcing"".""Streams"";");
    await pokemon.Database.ExecuteSqlRawAsync(sql.ToString());
  }
  protected virtual async Task InitializeDatabaseAsync()
  {
    Context.User = new UserBuilder(Faker).Build();
    UserClient.Setup(x => x.SearchAsync(
      It.Is<SearchUsersPayload>(p => p.Ids.Single() == Context.User.Id),
      It.IsAny<CancellationToken>())).ReturnsAsync(new SearchResults<User>([Context.User]));

    IWorldRepository worldRepository = ServiceProvider.GetRequiredService<IWorldRepository>();
    Context.World = new WorldBuilder(Faker).WithOwner(Context.User).Build();
    await worldRepository.SaveAsync(Context.World);
  }

  public virtual Task DisposeAsync() => Task.CompletedTask;
}
