namespace PokeGame.Core.Membership;

public sealed class MemberInvitationExpiredException : Exception
{
  public MemberInvitationExpiredException(MemberInvitation invitation) : base("The specified member invitation is expired.")
  {
    Data["InvitationId"] = invitation.EntityId;
    Data["WorldId"] = invitation.WorldId.EntityId;
    Data["ExpiresOn"] = invitation.ExpiresOn;
  }
}
