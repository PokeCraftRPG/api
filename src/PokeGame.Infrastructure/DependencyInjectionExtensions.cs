using Logitar.CQRS;
using Logitar.EventSourcing.EntityFrameworkCore.Relational;
using Logitar.EventSourcing.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure.Commands;
using PokeGame.Infrastructure.Repositories;

namespace PokeGame.Infrastructure;

public static class DependencyInjectionExtensions
{
  public static IServiceCollection AddPokeGameInfrastructure(this IServiceCollection services)
  {
    return services
      .AddLogitarEventSourcingWithEntityFrameworkCoreRelational()
      .AddRepositories()
      .AddSingleton<IEventSerializer, EventSerializer>()
      .AddScoped<IEventBus, EventBus>()
      .AddTransient<ICommandHandler<MigrateDatabaseCommand, Unit>, MigrateDatabaseCommandHandler>();
  }

  private static IServiceCollection AddRepositories(this IServiceCollection services)
  {
    return services.AddScoped<IWorldRepository, WorldRepository>();
  }
}
