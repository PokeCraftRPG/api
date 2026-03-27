using PokeGame.Core.Forms;

namespace PokeGame.Infrastructure.Converters;

internal class HeightConverter : JsonConverter<Height>
{
  public override Height Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    return new Height(reader.GetInt32());
  }

  public override void Write(Utf8JsonWriter writer, Height height, JsonSerializerOptions options)
  {
    writer.WriteNumberValue(height.Value);
  }
}
