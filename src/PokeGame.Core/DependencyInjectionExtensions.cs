using Logitar.CQRS;
using Logitar.EventSourcing;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Abilities;
using PokeGame.Core.Moves;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions;
using PokeGame.Core.Storages;
using PokeGame.Core.Worlds;

namespace PokeGame.Core;

public static class DependencyInjectionExtensions
{
  public static IServiceCollection AddPokeGameCore(this IServiceCollection services)
  {
    AbilityService.Register(services);
    MoveService.Register(services);
    RegionService.Register(services);
    WorldService.Register(services);
    return services
      .AddLogitarCQRS() // TODO(fpion): RetrySettings, IQueryBus
      .AddLogitarEventSourcing()
      .AddTransient<ICommandBus, CommandBus>()
      .AddTransient<IPermissionService, PermissionService>()
      .AddTransient<IStorageService, StorageService>();
  }
}
