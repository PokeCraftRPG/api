namespace PokeGame.Core.Membership;

public sealed class InvalidMemberInvitationStatusException : ConflictException
{
  public InvalidMemberInvitationStatusException(MemberInvitation invitation)
    : base("The specified member invitation status is not valid.")
  {
    Data["InvitationId"] = invitation.EntityId;
    Data["WorldId"] = invitation.WorldId.EntityId;
    Data["Status"] = invitation.Status;
  }
}
