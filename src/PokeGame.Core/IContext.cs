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

  IReadOnlyCollection<CustomAttribute> GetSessionCustomAttributes();

  Guid? TryGetSessionId();
  UserId? TryGetUserId();
  WorldId? TryGetWorldId();
}
