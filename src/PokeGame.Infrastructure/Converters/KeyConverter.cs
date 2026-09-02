using PokeGame.Core;

namespace PokeGame.Infrastructure.Converters;

internal class KeyConverter : JsonConverter<Key>
{
  public override Key? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    string? value = reader.GetString();
    return string.IsNullOrWhiteSpace(value) ? null : new Key(value);
  }

  public override void Write(Utf8JsonWriter writer, Key key, JsonSerializerOptions options)
  {
    writer.WriteStringValue(key.Value);
  }
}
