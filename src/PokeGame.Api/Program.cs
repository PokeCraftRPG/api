using Logitar.CQRS;
using PokeGame.Core.Identity;
using PokeGame.Infrastructure;
using PokeGame.Infrastructure.Caching;

namespace PokeGame.Api;

internal class Program
{
  public static async Task Main(string[] args)
  {
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    Startup startup = new(builder.Configuration);
    startup.ConfigureServices(builder.Services);

    WebApplication application = builder.Build();

    startup.Configure(application);

    using IServiceScope scope = application.Services.CreateScope();
    await MigrateDatabaseAsync(scope);
    await LoadRealmAsync(scope);

    application.Run();
  }

  private static async Task MigrateDatabaseAsync(IServiceScope scope, CancellationToken cancellationToken = default)
  {
    ICommandBus commandBus = scope.ServiceProvider.GetRequiredService<ICommandBus>();
    await commandBus.ExecuteAsync(new MigrateDatabaseCommand(), cancellationToken);
  }

  private static async Task LoadRealmAsync(IServiceScope scope, CancellationToken cancellationToken = default)
  {
    ICacheService cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
    IRealmGateway realmGateway = scope.ServiceProvider.GetRequiredService<IRealmGateway>();
    cacheService.Realm = await realmGateway.FindAsync(cancellationToken);
  }
}
