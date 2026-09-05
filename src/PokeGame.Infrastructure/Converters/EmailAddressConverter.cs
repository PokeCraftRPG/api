using PokeGame.Core.Identity;

namespace PokeGame.Infrastructure.Converters;

internal class EmailAddressConverter : JsonConverter<EmailAddress>
{
  public override EmailAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    string? value = reader.GetString();
    return string.IsNullOrWhiteSpace(value) ? null : new EmailAddress(value);
  }

  public override void Write(Utf8JsonWriter writer, EmailAddress emailAddress, JsonSerializerOptions options)
  {
    writer.WriteStringValue(emailAddress.Value);
  }
}
