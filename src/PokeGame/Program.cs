using Krakenar.Contracts.Realms;
using Logitar.CQRS;
using PokeGame.Core.Caching;
using PokeGame.Core.Identity;
using PokeGame.Infrastructure.Commands;

namespace PokeGame;

internal class Program
{
  public static async Task Main(string[] args)
  {
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
    IConfiguration configuration = builder.Configuration;

    Startup startup = new(configuration);
    startup.ConfigureServices(builder.Services);

    WebApplication application = builder.Build();

    startup.Configure(application);

    await MigrateDatabaseAsync(application.Services);
    await LoadRealmAsync(application.Services);

    application.Run();
  }

  private static async Task MigrateDatabaseAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
  {
    using IServiceScope scope = serviceProvider.CreateScope();
    ICommandBus commandBus = scope.ServiceProvider.GetRequiredService<ICommandBus>();
    await commandBus.ExecuteAsync(new MigrateDatabaseCommand(), cancellationToken);
  }

  private static async Task LoadRealmAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
  {
    IRealmGateway realmGateway = serviceProvider.GetRequiredService<IRealmGateway>();
    Realm realm = await realmGateway.FindAsync(cancellationToken);

    ICacheService cacheService = serviceProvider.GetRequiredService<ICacheService>();
    cacheService.RealmId = realm.Id;
  }
}
