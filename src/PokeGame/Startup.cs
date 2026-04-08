using Krakenar.Client;
using Krakenar.Contracts.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using PokeGame.Authentication;
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

    services.AddPokeGameCore();
    services.AddPokeGameInfrastructure();
    services.AddPokeGamePostgreSQL(_configuration);
    services.AddSingleton<IContext, HttpApplicationContext>();
    services.AddKrakenarClient(_configuration);

    services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    services.AddHttpContextAccessor();

    services.AddSingleton(CorsSettings.Initialize(_configuration));
    services.AddCors();

    ApiSettings apiSettings = ApiSettings.Initialize(_configuration);
    services.AddSingleton(apiSettings);
    if (apiSettings.EnableSwagger)
    {
      services.AddPokeGameSwagger(apiSettings);
    }

    AuthenticationSettings authenticationSettings = AuthenticationSettings.Initialize(_configuration);
    services.AddSingleton(authenticationSettings);
    string[] authenticationSchemes = GetAuthenticationSchemes(authenticationSettings);
    AuthenticationBuilder authenticationBuilder = services.AddAuthentication()
      .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(Schemes.ApiKey, options => { })
      .AddScheme<BearerAuthenticationOptions, BearerAuthenticationHandler>(Schemes.Bearer, options => { })
      .AddScheme<SessionAuthenticationOptions, SessionAuthenticationHandler>(Schemes.Session, options => { });
    if (authenticationSettings.EnableBasic)
    {
      authenticationBuilder.AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>(Schemes.Basic, options => { });
    }
    services.AddSingleton<IOpenAuthenticationService, OpenAuthenticationService>();

    services.AddAuthorizationBuilder().SetDefaultPolicy(new AuthorizationPolicyBuilder(authenticationSchemes).RequireAuthenticatedUser().Build());

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

    services.AddHealthChecks().AddDbContextCheck<PokemonContext>();
  }
  private static string[] GetAuthenticationSchemes(AuthenticationSettings settings)
  {
    List<string> schemes = new(capacity: 4)
    {
      Schemes.ApiKey,
      Schemes.Bearer,
      Schemes.Session
    };

    if (settings.EnableBasic)
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
  public virtual void Configure(WebApplication application)
  {
    ApiSettings apiSettings = application.Services.GetRequiredService<ApiSettings>();
    CorsSettings corsSettings = application.Services.GetRequiredService<CorsSettings>();

    if (apiSettings.EnableSwagger)
    {
      application.UsePokeGameSwagger(apiSettings);
    }

    application.UseHttpsRedirection();
    application.UseCors(corsSettings);
    application.UseExceptionHandler();
    application.UseSession();
    application.UseMiddleware<RenewSession>();
    application.UseAuthentication();
    application.UseAuthorization();
    application.UseMiddleware<ResolveWorld>();

    application.MapControllers();
    application.MapHealthChecks("/health");
  }
}
