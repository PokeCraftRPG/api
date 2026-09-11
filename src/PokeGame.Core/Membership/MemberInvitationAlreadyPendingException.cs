using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership;

public sealed class MemberInvitationAlreadyPendingException : ConflictException
{
  public MemberInvitationAlreadyPendingException(World world, MemberInvitationId invitationId)
    : base("A pending member invitation already exists and has not expired.")
  {
    Data["InvitationId"] = invitationId.EntityId;
    Data["WorldId"] = world.EntityId;
  }
}
