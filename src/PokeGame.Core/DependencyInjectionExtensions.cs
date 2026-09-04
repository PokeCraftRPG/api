using Logitar.CQRS;
using Logitar.EventSourcing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Abilities;
using PokeGame.Core.Assets;
using PokeGame.Core.Identity;
using PokeGame.Core.Moves;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Core;

public static class DependencyInjectionExtensions
{
  public static IServiceCollection AddPokeGameCore(this IServiceCollection services)
  {
    return services
      .AddCoreServices()
      .AddLogitarEventSourcing()
      .AddSingleton(serviceProvider => RetrySettings.Initialize(serviceProvider.GetRequiredService<IConfiguration>()))
      .AddTransient<ICommandBus, CommandBus>()
      .AddTransient<IQueryBus, QueryBus>();
  }

  private static IServiceCollection AddCoreServices(this IServiceCollection services)
  {
    AbilityService.Register(services);
    AssetService.Register(services);
    IdentityService.Register(services);
    MoveService.Register(services);
    PermissionService.Register(services);
    RegionService.Register(services);
    SpeciesService.Register(services);
    VarietyService.Register(services);
    WorldService.Register(services);
    return services;
  }
}
