using Logitar.CQRS;
using Logitar.EventSourcing.EntityFrameworkCore.Relational;
using Logitar.EventSourcing.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Identity;
using PokeGame.Core.Abilities;
using PokeGame.Core.Moves;
using PokeGame.Core.Regions;
using PokeGame.Core.Species;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Caching;
using PokeGame.Infrastructure.Commands;
using PokeGame.Infrastructure.Handlers;
using PokeGame.Infrastructure.Identity;
using PokeGame.Infrastructure.Outbox;
using PokeGame.Infrastructure.Queriers;
using PokeGame.Infrastructure.Repositories;

namespace PokeGame.Infrastructure;

public static class DependencyInjectionExtensions
{
  public static IServiceCollection AddPokeGameInfrastructure(this IServiceCollection services)
  {
    ActorService.Register(services);
    CacheService.Register(services);
    OutboxService.Register(services);

    return services
      .AddEventHandlers()
      .AddIdentityGateways()
      .AddLogitarEventSourcingWithEntityFrameworkCoreRelational()
      .AddMemoryCache()
      .AddQueriers()
      .AddRepositories()
      .AddSingleton(serviceProvider => TokensSettings.Initialize(serviceProvider.GetRequiredService<IConfiguration>()))
      .AddSingleton<IEventSerializer, EventSerializer>()
      .AddScoped<IEventBus, EventBus>()
      .AddTransient<ICommandHandler<MigrateDatabaseCommand, Unit>, MigrateDatabaseCommandHandler>();
  }

  private static IServiceCollection AddEventHandlers(this IServiceCollection services)
  {
    AbilityEvents.Register(services);
    MoveEvents.Register(services);
    RegionEvents.Register(services);
    SpeciesEvents.Register(services);
    WorldEvents.Register(services);
    return services;
  }

  private static IServiceCollection AddIdentityGateways(this IServiceCollection services)
  {
    return services
      .AddSingleton<IApiKeyGateway, ApiKeyGateway>()
      .AddSingleton<IRealmGateway, RealmGateway>()
      .AddSingleton<ISessionGateway, SessionGateway>()
      .AddSingleton<ITokenGateway, TokenGateway>()
      .AddSingleton<IUserGateway, UserGateway>();
  }

  private static IServiceCollection AddQueriers(this IServiceCollection services)
  {
    return services
      .AddScoped<IAbilityQuerier, AbilityQuerier>()
      .AddScoped<IMoveQuerier, MoveQuerier>()
      .AddScoped<IRegionQuerier, RegionQuerier>()
      .AddScoped<ISpeciesQuerier, SpeciesQuerier>()
      .AddScoped<IWorldQuerier, WorldQuerier>();
  }

  private static IServiceCollection AddRepositories(this IServiceCollection services)
  {
    return services
      .AddScoped<IAbilityRepository, AbilityRepository>()
      .AddScoped<IMoveRepository, MoveRepository>()
      .AddScoped<IRegionRepository, RegionRepository>()
      .AddScoped<ISpeciesRepository, SpeciesRepository>()
      .AddScoped<IWorldRepository, WorldRepository>();
  }
}
