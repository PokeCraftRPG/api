using PokeGame.Core.Regions;

namespace PokeGame.Infrastructure.Converters;

internal class LocationConverter : JsonConverter<Location>
{
  public override Location? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    return Location.TryCreate(reader.GetString());
  }

  public override void Write(Utf8JsonWriter writer, Location value, JsonSerializerOptions options)
  {
    writer.WriteStringValue(value.Value);
  }
}
