using Logitar;
using Microsoft.Extensions.Configuration;

namespace PokeGame.Infrastructure.Assets;

public sealed record StorageSettings
{
  private const string SectionKey = "Storage";

  public string RootPath { get; set; } = string.Empty;

  public static StorageSettings Initialize(IConfiguration configuration)
  {
    StorageSettings settings = configuration.GetSection(SectionKey).Get<StorageSettings>() ?? new();

    settings.RootPath = EnvironmentHelper.GetString("STORAGE_ROOT_PATH", settings.RootPath);

    return settings;
  }
}
