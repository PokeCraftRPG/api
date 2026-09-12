using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership;

public sealed class WorldOwnershipCannotBeRevokedException : ConflictException
{
  public WorldOwnershipCannotBeRevokedException(World world)
    : base("The world ownership cannot be revoked.")
  {
    Data["WorldId"] = world.EntityId;
    Data["OwnerId"] = world.OwnerId.EntityId;
  }
}
