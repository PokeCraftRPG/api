using Logitar.EventSourcing.EntityFrameworkCore.Relational;
using Logitar.EventSourcing.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Abilities;
using PokeGame.Core.Caching;
using PokeGame.Core.Regions;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Caching;
using PokeGame.Infrastructure.Handlers;
using PokeGame.Infrastructure.Queriers;
using PokeGame.Infrastructure.Repositories;
using PokeGame.Infrastructure.Settings;

namespace PokeGame.Infrastructure;

public static class DependencyInjectionExtensions
{
  public static IServiceCollection AddPokeGameInfrastructure(this IServiceCollection services)
  {
    return services
      .AddLogitarEventSourcingWithEntityFrameworkCoreRelational()
      .AddMemoryCache()
      .AddEventHandlers()
      .AddQueriers()
      .AddRepositories()
      .AddSingleton(serviceProvider => CachingSettings.Initialize(serviceProvider.GetRequiredService<IConfiguration>()))
      .AddSingleton<ICacheService, CacheService>()
      .AddSingleton<IEventBus, EventBus>()
      .AddSingleton<IEventSerializer, EventSerializer>()
      .AddTransient<IActorService, ActorService>();
  }

  private static IServiceCollection AddEventHandlers(this IServiceCollection services)
  {
    AbilityEvents.Register(services);
    RegionEvents.Register(services);
    WorldEvents.Register(services);
    return services;
  }

  private static IServiceCollection AddQueriers(this IServiceCollection services)
  {
    return services
      .AddTransient<IAbilityQuerier, AbilityQuerier>()
      .AddTransient<IRegionQuerier, RegionQuerier>()
      .AddTransient<IWorldQuerier, WorldQuerier>();
  }

  private static IServiceCollection AddRepositories(this IServiceCollection services)
  {
    return services
      .AddTransient<IAbilityRepository, AbilityRepository>()
      .AddTransient<IRegionRepository, RegionRepository>()
      .AddTransient<IWorldRepository, WorldRepository>();
  }
}
