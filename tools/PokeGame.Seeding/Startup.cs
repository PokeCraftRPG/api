using Krakenar.Client;
using Logitar.CQRS;
using PokeGame.Seeding.Krakenar.Tasks;

namespace PokeGame.Seeding;

internal class Startup
{
  private readonly IConfiguration _configuration;

  public Startup(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public void ConfigureServices(IServiceCollection services)
  {
    services.AddKrakenarClient(_configuration);
    services.AddTransient<ICommandBus, CommandBus>();

    services.AddHostedService<SeedingWorker>();

    services.AddTransient<ICommandHandler<SeedDictionariesTask, Unit>, SeedDictionariesTaskHandler>();
    services.AddTransient<ICommandHandler<SeedLanguagesTask, Unit>, SeedLanguagesTaskHandler>();
    services.AddTransient<ICommandHandler<SeedRealmTask, Unit>, SeedRealmTaskHandler>();
    services.AddTransient<ICommandHandler<SeedSendersTask, Unit>, SeedSendersTaskHandler>();
    services.AddTransient<ICommandHandler<SeedTemplatesTask, Unit>, SeedTemplatesTaskHandler>();
  }
}
