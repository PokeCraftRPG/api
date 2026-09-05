using PokeGame.Core.Membership;

namespace PokeGame.Infrastructure.Converters;

internal class MemberInvitationIdConverter : JsonConverter<MemberInvitationId>
{
  public override MemberInvitationId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    string? value = reader.GetString();
    return string.IsNullOrWhiteSpace(value) ? new MemberInvitationId() : new(value);
  }

  public override void Write(Utf8JsonWriter writer, MemberInvitationId invitationId, JsonSerializerOptions options)
  {
    writer.WriteStringValue(invitationId.Value);
  }
}
