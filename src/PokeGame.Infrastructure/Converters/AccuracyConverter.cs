using PokeGame.Core.Moves;

namespace PokeGame.Infrastructure.Converters;

internal class AccuracyConverter : JsonConverter<Accuracy>
{
  public override Accuracy? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    return reader.TryGetInt32(out int value) ? new Accuracy(value) : null;
  }

  public override void Write(Utf8JsonWriter writer, Accuracy accuracy, JsonSerializerOptions options)
  {
    writer.WriteNumberValue(accuracy.Value);
  }
}
