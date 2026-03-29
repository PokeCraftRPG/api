using Bogus;
using Krakenar.Client.Sessions;
using Krakenar.Client.Users;
using Krakenar.Contracts.Actors;
using Logitar;
using Logitar.CQRS;
using Logitar.EventSourcing.EntityFrameworkCore.Relational;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Caching;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure;
using PokeGame.PostgreSQL;

namespace PokeGame;

public abstract class IntegrationTests : IAsyncLifetime
{
  private readonly TestContext _context = new();

  protected Faker Faker { get; } = new();
  protected IServiceProvider ServiceProvider { get; }
  protected Mock<ISessionClient> SessionClient { get; } = new();
  protected Mock<IUserClient> UserClient { get; } = new();

  protected Actor Actor => _context.Actor;
  protected World World => _context.World ?? throw new InvalidOperationException("The world has not been initialized.");

  protected IntegrationTests()
  {
    IConfiguration configuration = new ConfigurationBuilder()
      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
      .Build();
    string connectionString = EnvironmentHelper.TryGetString("POSTGRESQLCONNSTR_Pokemon")
      ?? configuration.GetConnectionString("PostgreSQL")
      ?.Replace("{Database}", GetType().Name) ?? string.Empty;

    ServiceCollection services = new();
    services.AddSingleton(configuration);
    services.AddSingleton<IContext>(_context);

    services.AddPokeGameCore();
    services.AddPokeGameInfrastructure();
    services.AddPokeGamePostgreSQL(connectionString);

    services.AddSingleton(SessionClient.Object);
    services.AddSingleton(UserClient.Object);

    ServiceProvider = services.BuildServiceProvider();
  }

  public virtual async Task InitializeAsync()
  {
    ICommandBus commandBus = ServiceProvider.GetRequiredService<ICommandBus>();
    await commandBus.ExecuteAsync(new MigrateDatabaseCommand());

    PokemonContext pokemon = ServiceProvider.GetRequiredService<PokemonContext>();
    await pokemon.Forms.ExecuteDeleteAsync();
    await pokemon.Varieties.ExecuteDeleteAsync();
    await pokemon.Species.ExecuteDeleteAsync();
    await pokemon.Trainers.ExecuteDeleteAsync();
    await pokemon.Regions.ExecuteDeleteAsync();
    await pokemon.Moves.ExecuteDeleteAsync();
    await pokemon.Abilities.ExecuteDeleteAsync();
    await pokemon.Worlds.ExecuteDeleteAsync();

    EventContext events = ServiceProvider.GetRequiredService<EventContext>();
    await events.Events.ExecuteDeleteAsync();
    await events.Streams.ExecuteDeleteAsync();

    _context.User = new UserBuilder(Faker).Build();
    ICacheService cacheService = ServiceProvider.GetRequiredService<ICacheService>();
    cacheService.SetActor(new Actor(_context.User));

    _context.World = new WorldBuilder(Faker).WithUser(_context.User).Build();
    IWorldRepository worldRepository = ServiceProvider.GetRequiredService<IWorldRepository>();
    await worldRepository.SaveAsync(_context.World);
  }

  public virtual Task DisposeAsync() => Task.CompletedTask;
}
