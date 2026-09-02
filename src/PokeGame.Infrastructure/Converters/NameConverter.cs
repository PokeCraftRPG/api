using PokeGame.Core;

namespace PokeGame.Infrastructure.Converters;

internal class NameConverter : JsonConverter<Name>
{
  public override Name? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    return Name.TryCreate(reader.GetString());
  }

  public override void Write(Utf8JsonWriter writer, Name name, JsonSerializerOptions options)
  {
    writer.WriteStringValue(name.Value);
  }
}
