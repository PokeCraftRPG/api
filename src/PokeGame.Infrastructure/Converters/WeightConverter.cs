using PokeGame.Core.Forms;

namespace PokeGame.Infrastructure.Converters;

internal class WeightConverter : JsonConverter<Weight>
{
  public override Weight Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    return new Weight(reader.GetInt32());
  }

  public override void Write(Utf8JsonWriter writer, Weight weight, JsonSerializerOptions options)
  {
    writer.WriteNumberValue(weight.Value);
  }
}
