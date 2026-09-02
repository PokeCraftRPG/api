using Logitar;
using Logitar.CQRS;
using Logitar.EventSourcing.EntityFrameworkCore.PostgreSQL;
using Logitar.EventSourcing.EntityFrameworkCore.Relational;
using Logitar.EventSourcing.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Abilities;
using PokeGame.Core.Identity;
using PokeGame.Core.Regions;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Caching;
using PokeGame.Infrastructure.Handlers;
using PokeGame.Infrastructure.Identity;
using PokeGame.Infrastructure.Queriers;
using PokeGame.Infrastructure.Repositories;

namespace PokeGame.Infrastructure;

public static class DependencyInjectionExtensions
{
  public static IServiceCollection AddPokeGameInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    string? connectionString = EnvironmentHelper.TryGetString("POSTGRESQLCONNSTR_Pokemon") ?? configuration.GetConnectionString("PostgreSQL");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
      throw new InvalidOperationException("The PostgreSQL connection string was not found.");
    }
    return services.AddPokeGameInfrastructure(connectionString);
  }
  public static IServiceCollection AddPokeGameInfrastructure(this IServiceCollection services, string connectionString)
  {
    ActorService.Register(services);
    CacheService.Register(services);

    return services
      .AddDbContext<PokemonContext>(options => options.UseNpgsql(connectionString, options => options.MigrationsAssembly("PokeGame.Infrastructure")))
      .AddEventHandlers()
      .AddIdentityGateways()
      .AddLogitarEventSourcingWithEntityFrameworkCoreRelational()
      .AddLogitarEventSourcingWithEntityFrameworkCorePostgreSQL(connectionString)
      .AddQueriers()
      .AddRepositories()
      .AddSingleton<IEventSerializer, EventSerializer>()
      .AddScoped<ICommandHandler<MigrateDatabaseCommand, Unit>, MigrateDatabaseCommandHandler>()
      .AddScoped<IEventBus, EventBus>();
  }

  private static IServiceCollection AddEventHandlers(this IServiceCollection services)
  {
    AbilityEvents.Register(services);
    RegionEvents.Register(services);
    WorldEvents.Register(services);
    return services;
  }

  private static IServiceCollection AddIdentityGateways(this IServiceCollection services)
  {
    return services
      .AddSingleton(serviceProvider => ClientAppSettings.Initialize(serviceProvider.GetRequiredService<IConfiguration>()))
      .AddSingleton(serviceProvider => TokensSettings.Initialize(serviceProvider.GetRequiredService<IConfiguration>()))
      .AddSingleton<IApiKeyGateway, ApiKeyGateway>()
      .AddSingleton<IMessageGateway, MessageGateway>()
      .AddSingleton<IOneTimePasswordGateway, OneTimePasswordGateway>()
      .AddSingleton<IRealmGateway, RealmGateway>()
      .AddSingleton<ISessionGateway, SessionGateway>()
      .AddSingleton<ITokenGateway, TokenGateway>()
      .AddSingleton<IUserGateway, UserGateway>();
  }

  private static IServiceCollection AddQueriers(this IServiceCollection services)
  {
    return services
      .AddScoped<IAbilityQuerier, AbilityQuerier>()
      .AddScoped<IRegionQuerier, RegionQuerier>()
      .AddScoped<IWorldQuerier, WorldQuerier>();
  }

  private static IServiceCollection AddRepositories(this IServiceCollection services)
  {
    return services
      .AddScoped<IAbilityRepository, AbilityRepository>()
      .AddScoped<IRegionRepository, RegionRepository>()
      .AddScoped<IWorldRepository, WorldRepository>();
  }
}
