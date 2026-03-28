using Logitar.CQRS;
using Logitar.EventSourcing.EntityFrameworkCore.Relational;
using Logitar.EventSourcing.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Abilities;
using PokeGame.Core.Caching;
using PokeGame.Core.Forms;
using PokeGame.Core.Identity;
using PokeGame.Core.Moves;
using PokeGame.Core.Regions;
using PokeGame.Core.Species;
using PokeGame.Core.Trainers;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Caching;
using PokeGame.Infrastructure.Handlers;
using PokeGame.Infrastructure.Identity;
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
      .AddKrakenarGateways()
      .AddQueriers()
      .AddRepositories()
      .AddSingleton(serviceProvider => CachingSettings.Initialize(serviceProvider.GetRequiredService<IConfiguration>()))
      .AddSingleton<ICacheService, CacheService>()
      .AddSingleton<IEventBus, EventBus>()
      .AddSingleton<IEventSerializer, EventSerializer>()
      .AddTransient<IActorService, ActorService>()
      .AddTransient<ICommandHandler<MigrateDatabaseCommand, Unit>, MigrateDatabaseCommandHandler>();
  }

  private static IServiceCollection AddEventHandlers(this IServiceCollection services)
  {
    AbilityEvents.Register(services);
    FormEvents.Register(services);
    MoveEvents.Register(services);
    RegionEvents.Register(services);
    SpeciesEvents.Register(services);
    TrainerEvents.Register(services);
    VarietyEvents.Register(services);
    WorldEvents.Register(services);
    return services;
  }

  private static IServiceCollection AddKrakenarGateways(this IServiceCollection services)
  {
    return services
      .AddSingleton<IMessageGateway, MessageGateway>()
      .AddSingleton<IOneTimePasswordGateway, OneTimePasswordGateway>()
      .AddSingleton<ISessionGateway, SessionGateway>()
      .AddSingleton<ITokenGateway, TokenGateway>()
      .AddSingleton<IUserGateway, UserGateway>();
  }

  private static IServiceCollection AddQueriers(this IServiceCollection services)
  {
    return services
      .AddTransient<IAbilityQuerier, AbilityQuerier>()
      .AddTransient<IFormQuerier, FormQuerier>()
      .AddTransient<IMoveQuerier, MoveQuerier>()
      .AddTransient<IRegionQuerier, RegionQuerier>()
      .AddTransient<ISpeciesQuerier, SpeciesQuerier>()
      .AddTransient<ITrainerQuerier, TrainerQuerier>()
      .AddTransient<IVarietyQuerier, VarietyQuerier>()
      .AddTransient<IWorldQuerier, WorldQuerier>();
  }

  private static IServiceCollection AddRepositories(this IServiceCollection services)
  {
    return services
      .AddTransient<IAbilityRepository, AbilityRepository>()
      .AddTransient<IFormRepository, FormRepository>()
      .AddTransient<IMoveRepository, MoveRepository>()
      .AddTransient<IRegionRepository, RegionRepository>()
      .AddTransient<ISpeciesRepository, SpeciesRepository>()
      .AddTransient<ITrainerRepository, TrainerRepository>()
      .AddTransient<IVarietyRepository, VarietyRepository>()
      .AddTransient<IWorldRepository, WorldRepository>();
  }
}
