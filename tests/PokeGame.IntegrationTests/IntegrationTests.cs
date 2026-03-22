using Bogus;
using Krakenar.Contracts.Actors;
using Logitar;
using Logitar.CQRS;
using Logitar.EventSourcing.EntityFrameworkCore.Relational;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core;
using PokeGame.Core.Caching;
using PokeGame.Infrastructure;
using PokeGame.PostgreSQL;

namespace PokeGame;

public abstract class IntegrationTests : IAsyncLifetime
{
  private readonly IntegrationContext _context = new();

  protected Faker Faker { get; } = new();
  protected IServiceProvider ServiceProvider { get; }

  protected Actor Actor => _context.Actor;

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

    ServiceProvider = services.BuildServiceProvider();
  }

  public virtual async Task InitializeAsync()
  {
    ICommandBus commandBus = ServiceProvider.GetRequiredService<ICommandBus>();
    await commandBus.ExecuteAsync(new MigrateDatabaseCommand());

    PokemonContext pokemon = ServiceProvider.GetRequiredService<PokemonContext>();
    await pokemon.Worlds.ExecuteDeleteAsync();

    EventContext events = ServiceProvider.GetRequiredService<EventContext>();
    await events.Events.ExecuteDeleteAsync();
    await events.Streams.ExecuteDeleteAsync();

    _context.Actor = new Actor(Faker.Person.FullName)
    {
      RealmId = Guid.NewGuid(),
      Id = Guid.NewGuid(),
      Type = ActorType.User,
      EmailAddress = Faker.Person.Email,
      PictureUrl = Faker.Person.Avatar
    };

    ICacheService cacheService = ServiceProvider.GetRequiredService<ICacheService>();
    cacheService.SetActor(_context.Actor);
  }

  public virtual Task DisposeAsync() => Task.CompletedTask;
}
