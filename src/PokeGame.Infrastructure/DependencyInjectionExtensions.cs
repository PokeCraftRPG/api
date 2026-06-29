using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Identity;
using PokeGame.Core.Regions;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Caching;
using PokeGame.Infrastructure.Identity;
using PokeGame.Infrastructure.Repositories;

namespace PokeGame.Infrastructure;

public static class DependencyInjectionExtensions
{
  public static IServiceCollection AddPokeGameInfrastructure(this IServiceCollection services)
  {
    ActorService.Register(services);
    CacheService.Register(services);
    return services
      .AddIdentityGateways()
      .AddRepositories()
      .AddTransient<ICommandHandler<MigrateDatabaseCommand, Unit>, MigrateDatabaseCommandHandler>();
  }

  private static IServiceCollection AddIdentityGateways(this IServiceCollection services)
  {
    return services.AddSingleton<IUserGateway, UserGateway>();
  }

  private static IServiceCollection AddRepositories(this IServiceCollection services)
  {
    return services
      .AddScoped<IRegionRepository, RegionRepository>()
      .AddScoped<IWorldRepository, WorldRepository>();
  }
}
