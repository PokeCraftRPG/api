using Krakenar.Client;
using Krakenar.Contracts.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;
using PokeGame.Api.Authentication;
using PokeGame.Api.Extensions;
using PokeGame.Api.Middlewares;
using PokeGame.Api.Settings;
using PokeGame.Core;
using PokeGame.Infrastructure;
using PokeGame.Infrastructure.Assets;

namespace PokeGame.Api;

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
    services.AddPokeGameInfrastructure(_configuration);
    services.AddSingleton<IContext, HttpApplicationContext>();

    services.AddKrakenarClient(_configuration);

    services.AddControllers().AddJsonOptions(options =>
    {
      options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
      options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
    services.AddHttpContextAccessor();

    services.AddSingleton(_corsSettings);
    services.AddCors();

    string[] authenticationSchemes = GetAuthenticationSchemes();
    AuthenticationBuilder authenticationBuilder = services
      .AddAuthentication()
      .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(Schemes.ApiKey, options => { })
      .AddScheme<BearerAuthenticationOptions, BearerAuthenticationHandler>(Schemes.Bearer, options => { })
      .AddScheme<SessionAuthenticationOptions, SessionAuthenticationHandler>(Schemes.Session, options => { });
    if (_apiSettings.EnableBasicAuthentication)
    {
      authenticationBuilder.AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>(Schemes.Basic, options => { });
    }

    services.AddAuthorizationBuilder().SetDefaultPolicy(new AuthorizationPolicyBuilder(authenticationSchemes).RequireAuthenticatedUser().Build());

    CookiesSettings cookiesSettings = CookiesSettings.Initialize(_configuration);
    services.AddSingleton(cookiesSettings);
    services.AddSession(options =>
    {
      options.Cookie.SameSite = cookiesSettings.Session.SameSite;
      options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });
    services.AddDistributedMemoryCache();

    services.AddExceptionHandler<ExceptionHandler>();
    services.AddProblemDetails();

    services.AddHealthChecks().AddDbContextCheck<PokemonContext>();

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
  public virtual void Configure(WebApplication application)
  {
    ApiSettings apiSettings = application.Services.GetRequiredService<ApiSettings>();
    if (apiSettings.EnableSwagger)
    {
      application.UsePokeGameSwagger(apiSettings);
    }

    application.UseExceptionHandler();
    application.UseHttpsRedirection();
    application.UseCors(_corsSettings);
    application.UseStaticFiles();
    UseStaticFiles(application);
    application.UseSession();
    application.UseMiddleware<RenewSession>();
    application.UseAuthentication();
    application.UseAuthorization();
    application.UseMiddleware<ResolveWorld>();

    application.MapControllers();
    application.MapHealthChecks("/health");
  }
  private static void UseStaticFiles(WebApplication application)
  {
    StorageSettings storageSettings = application.Services.GetRequiredService<StorageSettings>();
    application.UseStaticFiles(new StaticFileOptions
    {
      FileProvider = new PhysicalFileProvider(storageSettings.RootPath),
      RequestPath = "/assets"
    });
  }
}
