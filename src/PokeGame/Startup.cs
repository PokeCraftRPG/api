using PokeGame.Core;
using PokeGame.Infrastructure;
using PokeGame.PostgreSQL;

namespace PokeGame;

internal class Startup : StartupBase
{
  private readonly IConfiguration _configuration;

  public Startup(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public override void ConfigureServices(IServiceCollection services)
  {
    base.ConfigureServices(services);

    services.AddControllers();

    services.AddOpenApi();

    services.AddPokeGameCore();
    services.AddPokeGameInfrastructure();
    services.AddPokeGamePostgreSQL(_configuration);
    services.AddSingleton<IContext, HttpApplicationContext>();
  }

  public override void Configure(IApplicationBuilder builder)
  {
    if (builder is WebApplication application)
    {
      Configure(application);
    }
  }
  public virtual void Configure(WebApplication application)
  {
    if (application.Environment.IsDevelopment())
    {
      application.MapOpenApi();
    }

    application.UseHttpsRedirection();

    application.MapControllers();
  }
}
