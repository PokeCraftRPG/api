using Krakenar.Client;
using Krakenar.Contracts.Constants;
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
  private readonly ApiSettings _apiSettings;
  private readonly CorsSettings _corsSettings;

  public Startup(IConfiguration configuration)
  {
    _configuration = configuration;
    _apiSettings = ApiSettings.Initialize(configuration);
    _corsSettings = CorsSettings.Initialize(configuration);
  }

  public override void ConfigureServices(IServiceCollection services)
  {
    base.ConfigureServices(services);

    services.AddPokeGameCore();
    services.AddPokeGameInfrastructure();
    services.AddPokeGamePostgreSQL(_configuration);
    services.AddSingleton<IContext, HttpApplicationContext>();

    services.AddKrakenarClient(_configuration);

    services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    services.AddHttpContextAccessor();

    services.AddSingleton(_corsSettings);
    services.AddCors();

    CookiesSettings cookiesSettings = CookiesSettings.Initialize(_configuration);
    services.AddSingleton(cookiesSettings);
    services.AddSession(options =>
    {
      options.Cookie.SameSite = cookiesSettings.Session.SameSite;
      options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });
    services.AddDistributedMemoryCache();

    ErrorSettings settings = ErrorSettings.Initialize(_configuration);
    services.AddSingleton(settings);
    services.AddExceptionHandler<ExceptionHandler>();
    services.AddProblemDetails();

    services.AddHealthChecks();

    services.AddSingleton(_apiSettings);
    if (_apiSettings.EnableSwagger)
    {
      services.AddPokeGameSwagger(_apiSettings);
    }
  }
  private string[] GetAuthenticationSchemes()
  {
    List<string> schemes = new(capacity: 4)
    {
      Schemes.ApiKey,
      Schemes.Bearer,
      Schemes.Session
    };

    if (_apiSettings.EnableBasicAuthentication)
    {
      schemes.Add(Schemes.Basic);
    }

    return [.. schemes];
  }

  public override void Configure(IApplicationBuilder builder)
  {
    if (builder is WebApplication application)
    {
      Configure(application);
    }
  }
  public void Configure(WebApplication application)
  {
    if (_apiSettings.EnableSwagger)
    {
      application.UsePokeGameSwagger(_apiSettings);
    }
    application.UseHttpsRedirection();
    application.UseCors(_corsSettings);
    application.UseExceptionHandler();
    application.UseSession();
    application.UseAuthentication();
    application.UseAuthorization();
    application.UseMiddleware<ResolveWorld>();

    application.MapControllers();
  }
}
