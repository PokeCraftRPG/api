using Krakenar.Contracts;
using Logitar.EventSourcing;
using PokeGame.Core.Identity;
using PokeGame.Core.Worlds;

namespace PokeGame.Core;

public interface IContext
{
  ActorId? ActorId { get; }
  UserId UserId { get; }
  WorldId WorldId { get; }
  bool IsWorldOwner { get; }

  UserId? TryGetUserId();
  WorldId? TryGetWorldId();

  IReadOnlyCollection<CustomAttribute> GetSessionCustomAttributes();
}
