using Krakenar.Contracts.Actors;
using Logitar.EventSourcing;

namespace PokeGame.Core.Caching;

public interface ICacheService
{
  Actor? GetActor(ActorId id);
  void RemoveActor(ActorId id);
  void SetActor(Actor actor);
}
