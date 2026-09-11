using PokeGame.Core.Identity;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership;

public sealed class UserIsNotMemberException : ConflictException
{
  public UserIsNotMemberException(World world, UserId userId)
    : base("The specified user is not a member of this world.")
  {
    Data["WorldId"] = world.EntityId;
    Data["UserId"] = userId.EntityId;
  }
}
