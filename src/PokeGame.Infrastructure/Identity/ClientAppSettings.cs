using Logitar;
using Microsoft.Extensions.Configuration;

namespace PokeGame.Infrastructure.Identity;

internal record ClientAppSettings
{
  private const string SectionKey = "ClientApp";

  public string BaseUrl { get; set; } = string.Empty;
  public string EmailVerificationPath { get; set; } = string.Empty;

  public static ClientAppSettings Initialize(IConfiguration configuration)
  {
    ClientAppSettings settings = configuration.GetSection(SectionKey).Get<ClientAppSettings>() ?? new();

    settings.BaseUrl = EnvironmentHelper.GetString("CLIENT_APP_BASE_URL", settings.BaseUrl);
    settings.EmailVerificationPath = EnvironmentHelper.GetString("CLIENT_APP_EMAIL_VERIFICATION_PATH", settings.EmailVerificationPath);

    return settings;
  }
}
