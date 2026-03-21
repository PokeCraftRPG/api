using PokeGame.Core;

namespace PokeGame.Infrastructure.Converters;

internal class NotesConverter : JsonConverter<Notes>
{
  public override Notes? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    return Notes.TryCreate(reader.GetString());
  }

  public override void Write(Utf8JsonWriter writer, Notes notes, JsonSerializerOptions options)
  {
    writer.WriteStringValue(notes.Value);
  }
}
