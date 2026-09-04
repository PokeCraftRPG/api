using PokeGame.Core.Varieties;

namespace PokeGame.Infrastructure.Converters;

internal class GenusConverter : JsonConverter<Genus>
{
  public override Genus? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    return Genus.TryCreate(reader.GetString());
  }

  public override void Write(Utf8JsonWriter writer, Genus genus, JsonSerializerOptions options)
  {
    writer.WriteStringValue(genus.Value);
  }
}
