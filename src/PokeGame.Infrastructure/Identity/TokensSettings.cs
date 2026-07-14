using Logitar;
using Microsoft.Extensions.Configuration;

namespace PokeGame.Infrastructure.Identity;

internal record TokensSettings
{
  private const string SectionKey = "Tokens";

  public TokenSettings Access { get; set; } = new();
  public TokenSettings EmailVerification { get; set; } = new();
  public TokenSettings ProfileCompletion { get; set; } = new();

  public static TokensSettings Initialize(IConfiguration configuration)
  {
    TokensSettings settings = configuration.GetSection(SectionKey).Get<TokensSettings>() ?? new();

    settings.Access.LifetimeSeconds = EnvironmentHelper.GetInt32("TOKENS_ACCESS_LIFETIME_SECONDS", settings.Access.LifetimeSeconds);
    settings.EmailVerification.LifetimeSeconds = EnvironmentHelper.GetInt32("TOKENS_EMAIL_VERIFICATION_LIFETIME_SECONDS", settings.EmailVerification.LifetimeSeconds);
    settings.ProfileCompletion.LifetimeSeconds = EnvironmentHelper.GetInt32("TOKENS_PROFILE_COMPLETION_LIFETIME_SECONDS", settings.ProfileCompletion.LifetimeSeconds);

    return settings;
  }
}

internal record TokenSettings
{
  public int LifetimeSeconds { get; set; }
}
