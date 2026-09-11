using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership;

public sealed class OwnerCannotLeaveWorldException : ConflictException
{
  public OwnerCannotLeaveWorldException(World world)
    : base("The owner cannot leave this world.")
  {
    Data["WorldId"] = world.EntityId;
    Data["OwnerId"] = world.OwnerId.EntityId;
  }
}
