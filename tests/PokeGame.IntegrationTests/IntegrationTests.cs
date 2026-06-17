using Bogus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PokeGame;

public abstract class IntegrationTests : IAsyncLifetime
{
  protected virtual IConfiguration Configuration { get; }
  protected virtual IServiceProvider ServiceProvider { get; }

  protected virtual Faker Faker { get; } = new();

  protected IntegrationTests()
  {
    Configuration = BuildConfiguration();
    ServiceProvider = BuildServiceProvider();
  }

  protected virtual IConfiguration BuildConfiguration() => new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

  protected virtual IServiceProvider BuildServiceProvider()
  {
    ServiceCollection services = new();
    services.AddLogging();

    services.AddSingleton(Configuration);

    return services.BuildServiceProvider();
  }

  public virtual async Task InitializeAsync()
  {
    await MigrateDatabaseAsync();
    await ClearDatabaseAsync();
    await InitializeDatabaseAsync();
  }
  protected virtual async Task MigrateDatabaseAsync()
  {
  }
  protected virtual async Task ClearDatabaseAsync()
  {
  }
  protected virtual async Task InitializeDatabaseAsync()
  {
  }

  public virtual Task DisposeAsync() => Task.CompletedTask;
}
