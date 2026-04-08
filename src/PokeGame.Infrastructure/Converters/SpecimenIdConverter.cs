using PokeGame.Core.Pokemon;

namespace PokeGame.Infrastructure.Converters;

internal class SpecimenIdConverter : JsonConverter<SpecimenId>
{
  public override SpecimenId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    string? value = reader.GetString();
    return string.IsNullOrWhiteSpace(value) ? new SpecimenId() : new(value);
  }

  public override void Write(Utf8JsonWriter writer, SpecimenId specimenId, JsonSerializerOptions options)
  {
    writer.WriteStringValue(specimenId.Value);
  }
}
