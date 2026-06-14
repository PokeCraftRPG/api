using Logitar.CQRS;
using Logitar.EventSourcing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions;
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
    PermissionService.Register(services);
    RegionService.Register(services);
    WorldService.Register(services);
    return services;
  }
}
