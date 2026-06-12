using PokeGame.Core.Forms;

namespace PokeGame.Infrastructure.Converters;

internal class FormIdConverter : JsonConverter<FormId>
{
  public override FormId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    string? value = reader.GetString();
    return string.IsNullOrWhiteSpace(value) ? default : new(value);
  }

  public override void Write(Utf8JsonWriter writer, FormId formId, JsonSerializerOptions options)
  {
    writer.WriteStringValue(formId.Value);
  }
}
