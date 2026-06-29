using Bogus;
using Krakenar.Client.Users;
using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Krakenar.Contracts.Users;
using Logitar;
using Logitar.CQRS;
using Logitar.Data;
using Logitar.Data.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure;
using PokeGame.PostgreSQL;

namespace PokeGame;

public abstract class IntegrationTests : IAsyncLifetime
{
  protected virtual IConfiguration Configuration { get; }
  protected virtual TestContext Context { get; }
  protected virtual IServiceProvider ServiceProvider { get; }

  protected virtual Actor Actor => Context.User is null ? new() : new(Context.User);
  protected virtual Faker Faker { get; } = new();
  protected virtual Mock<IUserClient> UserClient { get; } = new();

  protected IntegrationTests()
  {
    Configuration = BuildConfiguration();
    Context = new TestContext(Faker);
    ServiceProvider = BuildServiceProvider();

    Context.Database = ServiceProvider.GetRequiredService<PokemonContext>();
  }

  protected virtual IConfiguration BuildConfiguration() => new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

  protected virtual IServiceProvider BuildServiceProvider()
  {
    ServiceCollection services = new();
    services.AddLogging();

    services.AddSingleton(Configuration);

    services.AddPokeGameCore();
    services.AddPokeGameInfrastructure();

    string? connectionString = EnvironmentHelper.TryGetString("POSTGRESQLCONNSTR_Pokemon")
      ?? Configuration.GetConnectionString("PostgreSQL")?.CleanTrim()
      ?? throw new InvalidOperationException("The PostgreSQL connection string was not found.");
    services.AddPokeGamePostgreSQL(connectionString.Replace("{Database}", GetType().Name));

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
    PokemonContext context = ServiceProvider.GetRequiredService<PokemonContext>();
    StringBuilder sql = new();
    TableId[] tables =
    [
      Infrastructure.Db.Regions.Table,
      Infrastructure.Db.Worlds.Table,
      Infrastructure.Db.History.Table
    ];
    foreach (TableId table in tables)
    {
      sql.Append(new PostgresDeleteBuilder(table).Build().Text).Append(';').AppendLine();
    }
    await context.Database.ExecuteSqlRawAsync(sql.ToString());
  }
  protected virtual async Task InitializeDatabaseAsync()
  {
    Context.User = new UserBuilder(Faker).Build();
    UserClient.Setup(x => x.SearchAsync(
      It.Is<SearchUsersPayload>(p => p.Ids.Single() == Context.User.Id),
      It.IsAny<CancellationToken>())).ReturnsAsync(new SearchResults<User>([Context.User]));

    IWorldRepository worldRepository = ServiceProvider.GetRequiredService<IWorldRepository>();
    Context.World = new WorldBuilder(Faker).WithOwner(Context.User).Build();
    worldRepository.Add(Context.World);
    await Context.SaveChangesAsync();
  }

  public virtual Task DisposeAsync() => Task.CompletedTask;
}
