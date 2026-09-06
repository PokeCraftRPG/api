using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Realms;
using Logitar.EventSourcing;

namespace PokeGame.Core.Caching;

public interface ICacheService
{
  Realm? Realm { get; set; }

  Actor? GetActor(ActorId id);
  void RemoveActor(ActorId id);
  void SetActor(Actor actor);
}
