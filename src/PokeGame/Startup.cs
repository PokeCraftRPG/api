using Krakenar.Client;
using PokeGame.Core;
using PokeGame.Extensions;
using PokeGame.Infrastructure;
using PokeGame.Middlewares;
using PokeGame.PostgreSQL;
using PokeGame.Settings;

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
    services.AddHttpContextAccessor();
    services.AddProblemDetails();

    ApiSettings apiSettings = ApiSettings.Initialize(_configuration);
    services.AddSingleton(apiSettings);
    if (apiSettings.EnableSwagger)
    {
      services.AddPokeGameSwagger(apiSettings);
    }

    services.AddPokeGameCore();
    services.AddPokeGameInfrastructure();
    services.AddPokeGamePostgreSQL(_configuration);
    services.AddSingleton<IContext, HttpApplicationContext>();
    services.AddKrakenarClient(_configuration);
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
    ApiSettings apiSettings = application.Services.GetRequiredService<ApiSettings>();
    if (apiSettings.EnableSwagger)
    {
      application.UsePokeGameSwagger(apiSettings);
    }

    application.UseHttpsRedirection();
    application.UseMiddleware<ResolveWorld>();

    application.MapControllers();
  }
}
