using PokeGame.Core;

namespace PokeGame.Infrastructure.Converters;

internal class ContentConverter : JsonConverter<Content>
{
  public override Content? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    return Content.TryCreate(reader.GetString());
  }

  public override void Write(Utf8JsonWriter writer, Content content, JsonSerializerOptions options)
  {
    writer.WriteStringValue(content.Value);
  }
}
