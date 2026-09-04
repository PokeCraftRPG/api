using Microsoft.Extensions.Configuration;

namespace PokeGame.Core.Assets.Settings;

internal record AssetSettings(AssetKind Kind, string Extension);

internal record AssetsSettings
{
  private const string SectionKey = "Assets";

  public IReadOnlyDictionary<string, AssetSettings> SupportedTypes { get; init; } = new Dictionary<string, AssetSettings>().AsReadOnly();

  public static AssetsSettings Initialize(IConfiguration configuration) => configuration.GetSection(SectionKey).Get<AssetsSettings>() ?? new();
}
