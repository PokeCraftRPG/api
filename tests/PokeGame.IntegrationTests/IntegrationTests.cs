using Bogus;
using Krakenar.Client;
using Krakenar.Client.Sessions;
using Krakenar.Client.Users;
using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Messages;
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
  protected TestContext Context { get; } = new();
  protected Faker Faker { get; } = new();
  protected IServiceProvider ServiceProvider { get; }
  protected Mock<IMessageService> MessageService { get; } = new();
  protected Mock<ISessionClient> SessionClient { get; } = new();
  protected Mock<IUserClient> UserClient { get; } = new();

  protected Actor Actor => Context.Actor;
  protected World World => Context.World ?? throw new InvalidOperationException("The world has not been initialized.");

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
    services.AddSingleton(KrakenarSettings.Initialize(configuration));
    services.AddSingleton<IContext>(Context);

    services.AddPokeGameCore();
    services.AddPokeGameInfrastructure();
    services.AddPokeGamePostgreSQL(connectionString);

    services.AddSingleton(MessageService.Object);
    services.AddSingleton(SessionClient.Object);
    services.AddSingleton(UserClient.Object);

    ServiceProvider = services.BuildServiceProvider();
  }

  public virtual async Task InitializeAsync()
  {
    ICommandBus commandBus = ServiceProvider.GetRequiredService<ICommandBus>();
    await commandBus.ExecuteAsync(new MigrateDatabaseCommand());

    PokemonContext pokemon = ServiceProvider.GetRequiredService<PokemonContext>();
    await pokemon.Evolutions.ExecuteDeleteAsync();
    await pokemon.Forms.ExecuteDeleteAsync();
    await pokemon.Varieties.ExecuteDeleteAsync();
    await pokemon.Species.ExecuteDeleteAsync();
    await pokemon.Items.ExecuteDeleteAsync();
    await pokemon.Trainers.ExecuteDeleteAsync();
    await pokemon.Regions.ExecuteDeleteAsync();
    await pokemon.Moves.ExecuteDeleteAsync();
    await pokemon.Abilities.ExecuteDeleteAsync();
    await pokemon.StorageSummary.ExecuteDeleteAsync();
    await pokemon.MembershipInvitations.ExecuteDeleteAsync();
    await pokemon.Worlds.ExecuteDeleteAsync();

    EventContext events = ServiceProvider.GetRequiredService<EventContext>();
    await events.Events.ExecuteDeleteAsync();
    await events.Streams.ExecuteDeleteAsync();

    Context.User = new UserBuilder(Faker).Build();
    UserClient.Setup(x => x.ReadAsync(Context.User.Id, uniqueName: null, customIdentifier: null, It.IsAny<RequestContext>())).ReturnsAsync(Context.User);
    ICacheService cacheService = ServiceProvider.GetRequiredService<ICacheService>();
    cacheService.SetActor(new Actor(Context.User));

    Context.World = new WorldBuilder(Faker).WithUser(Context.User).Build();
    IWorldRepository worldRepository = ServiceProvider.GetRequiredService<IWorldRepository>();
    await worldRepository.SaveAsync(Context.World);
  }

  public virtual Task DisposeAsync() => Task.CompletedTask;
}
