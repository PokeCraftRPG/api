using Logitar;

namespace PokeGame.Settings;

public record ApiSettings
{
  public const string SectionKey = "Api";

  public bool EnableBasicAuthentication { get; set; }
  public bool EnableSwagger { get; set; }
  public string Title { get; set; } = string.Empty;
  public string? Build { get; set; }
  public Version Version { get; set; } = new();

  public static ApiSettings Initialize(IConfiguration configuration)
  {
    ApiSettings settings = configuration.GetSection(SectionKey).Get<ApiSettings>() ?? new();

    settings.EnableBasicAuthentication = EnvironmentHelper.GetBoolean("ADMIN_ENABLE_BASIC_AUTHENTICATION", settings.EnableBasicAuthentication);
    settings.EnableSwagger = EnvironmentHelper.GetBoolean("ADMIN_ENABLE_SWAGGER", settings.EnableSwagger);
    settings.Title = EnvironmentHelper.GetString("ADMIN_TITLE", settings.Title);

    string? versionValue = EnvironmentHelper.TryGetString("ADMIN_VERSION");
    if (Version.TryParse(versionValue, out Version? version))
    {
      settings.Version = version;
    }

    settings.Build = EnvironmentHelper.TryGetString("API_BUILD") ?? settings.Build;

    return settings;
  }
}
