using Logitar;
using Microsoft.Extensions.Configuration;

namespace PokeGame.Core.Permissions;

internal record PermissionSettings
{
  private const string SectionKey = "Permissions";

  public int WorldLimit { get; set; }

  public static PermissionSettings Initialize(IConfiguration configuration)
  {
    PermissionSettings settings = configuration.GetSection(SectionKey).Get<PermissionSettings>() ?? new PermissionSettings();

    settings.WorldLimit = EnvironmentHelper.GetInt32("PERMISSIONS_WORLD_LIMIT", settings.WorldLimit);

    return settings;
  }
}
