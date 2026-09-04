using PokeGame.Core.Assets;

namespace PokeGame.Infrastructure.Converters;

internal class AssetIdConverter : JsonConverter<AssetId>
{
  public override AssetId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    string? value = reader.GetString();
    return string.IsNullOrWhiteSpace(value) ? new AssetId() : new(value);
  }

  public override void Write(Utf8JsonWriter writer, AssetId assetId, JsonSerializerOptions options)
  {
    writer.WriteStringValue(assetId.Value);
  }
}
