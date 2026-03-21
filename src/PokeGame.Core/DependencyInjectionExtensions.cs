using Logitar.CQRS;
using Logitar.EventSourcing;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Abilities;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;
using PokeGame.Core.Worlds;

namespace PokeGame.Core;

public static class DependencyInjectionExtensions
{
  public static IServiceCollection AddPokeGameCore(this IServiceCollection services)
  {
    AbilityService.Register(services);
    WorldService.Register(services);
    return services
      .AddLogitarCQRS()
      .AddLogitarEventSourcing()
      .AddTransient<IPermissionService, PermissionService>()
      .AddTransient<IStorageService, StorageService>();
  }
}
