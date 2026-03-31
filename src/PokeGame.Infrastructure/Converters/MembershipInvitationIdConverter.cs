using PokeGame.Core.Membership;

namespace PokeGame.Infrastructure.Converters;

internal class MembershipInvitationIdConverter : JsonConverter<MembershipInvitationId>
{
  public override MembershipInvitationId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    string? value = reader.GetString();
    return string.IsNullOrWhiteSpace(value) ? new MembershipInvitationId() : new(value);
  }

  public override void Write(Utf8JsonWriter writer, MembershipInvitationId membershipInvitationId, JsonSerializerOptions options)
  {
    writer.WriteStringValue(membershipInvitationId.Value);
  }
}
