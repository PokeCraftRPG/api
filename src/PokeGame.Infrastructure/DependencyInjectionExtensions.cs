using Logitar.CQRS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Abilities;
using PokeGame.Core.Identity;
using PokeGame.Core.Moves;
using PokeGame.Core.Regions;
using PokeGame.Core.Species;
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
      .AddSingleton(serviceProvider => ClientAppSettings.Initialize(serviceProvider.GetRequiredService<IConfiguration>()))
      .AddSingleton(serviceProvider => TokensSettings.Initialize(serviceProvider.GetRequiredService<IConfiguration>()))
      .AddTransient<ICommandHandler<MigrateDatabaseCommand, Unit>, MigrateDatabaseCommandHandler>();
  }

  private static IServiceCollection AddIdentityGateways(this IServiceCollection services)
  {
    return services
      .AddSingleton<IApiKeyGateway, ApiKeyGateway>()
      .AddSingleton<IMessageGateway, MessageGateway>()
      .AddSingleton<IOneTimePasswordGateway, OneTimePasswordGateway>()
      .AddSingleton<IRealmGateway, RealmGateway>()
      .AddSingleton<ISessionGateway, SessionGateway>()
      .AddSingleton<ITokenGateway, TokenGateway>()
      .AddSingleton<IUserGateway, UserGateway>();
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
