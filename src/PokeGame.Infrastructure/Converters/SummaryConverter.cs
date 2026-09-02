using PokeGame.Core;

namespace PokeGame.Infrastructure.Converters;

internal class SummaryConverter : JsonConverter<Summary>
{
  public override Summary? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    return Summary.TryCreate(reader.GetString());
  }

  public override void Write(Utf8JsonWriter writer, Summary summary, JsonSerializerOptions options)
  {
    writer.WriteStringValue(summary.Value);
  }
}
