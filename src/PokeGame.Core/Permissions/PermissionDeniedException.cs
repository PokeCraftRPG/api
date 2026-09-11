using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Permissions;

public sealed class PermissionDeniedException : Exception
{
  public PermissionDeniedException(ActorId? actorId, string action, Entity? entity, WorldId? worldId)
    : base("The specified permission was denied.")
  {
    Data["Principal"] = actorId?.Value;
    Data["Action"] = action;
    Data["Resource"] = entity?.ToString();
    Data["WorldId"] = worldId;
  }
}
