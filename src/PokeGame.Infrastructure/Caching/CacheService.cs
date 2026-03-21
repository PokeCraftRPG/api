using Krakenar.Contracts.Actors;
using Logitar.EventSourcing;
using Microsoft.Extensions.Caching.Memory;
using PokeGame.Core.Actors;
using PokeGame.Core.Caching;
using PokeGame.Infrastructure.Settings;

namespace PokeGame.Infrastructure.Caching;

internal class CacheService : ICacheService
{
  private readonly IMemoryCache _memoryCache;
  private readonly CachingSettings _settings;

  public CacheService(IMemoryCache memoryCache, CachingSettings settings)
  {
    _memoryCache = memoryCache;
    _settings = settings;
  }

  public Actor? GetActor(ActorId id)
  {
    string key = GetActorKey(id);
    return _memoryCache.TryGetValue(key, out object? value) ? (Actor?)value : null;
  }
  public void RemoveActor(ActorId id)
  {
    string key = GetActorKey(id);
    _memoryCache.Remove(key);
  }
  public void SetActor(Actor actor)
  {
    ActorId actorId = actor.GetActorId();
    string key = GetActorKey(actorId);
    _memoryCache.Set(key, actor, _settings.ActorLifetime);
  }
  private static string GetActorKey(ActorId actorId) => $"Actor.Id={actorId}";
}
