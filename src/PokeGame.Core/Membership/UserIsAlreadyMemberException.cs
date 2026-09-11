using PokeGame.Core.Identity;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership;

public sealed class UserIsAlreadyMemberException : ConflictException
{
  public UserIsAlreadyMemberException(World world, UserId userId)
    : base("The specified user is already a member of this world.")
  {
    Data["WorldId"] = world.EntityId;
    Data["UserId"] = userId.EntityId;
  }
}
