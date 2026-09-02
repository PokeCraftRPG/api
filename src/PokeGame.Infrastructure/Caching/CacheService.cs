using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Realms;
using Logitar.EventSourcing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Infrastructure.Actors;

namespace PokeGame.Infrastructure.Caching;

public interface ICacheService
{
  Realm? Realm { get; set; }

  Actor? GetActor(ActorId id);
  void RemoveActor(ActorId id);
  void SetActor(Actor actor);
}

internal class CacheService : ICacheService
{
  public static void Register(IServiceCollection services)
  {
    services.AddMemoryCache();
    services.AddSingleton(serviceProvider => CachingSettings.Initialize(serviceProvider.GetRequiredService<IConfiguration>()));
    services.AddSingleton<ICacheService, CacheService>();
  }

  private readonly IMemoryCache _cache;
  private readonly CachingSettings _settings;

  public CacheService(IMemoryCache cache, CachingSettings settings)
  {
    _cache = cache;
    _settings = settings;
  }

  public Realm? Realm
  {
    get => _cache.TryGetValue(RealmKey, out object? value) ? (Realm?)value : null;
    set => _cache.Set(RealmKey, value);
  }
  private const string RealmKey = "Realm";

  public Actor? GetActor(ActorId id)
  {
    string key = GetActorKey(id);
    return _cache.TryGetValue(key, out object? value) ? (Actor?)value : null;
  }
  public void RemoveActor(ActorId id)
  {
    string key = GetActorKey(id);
    _cache.Remove(key);
  }
  public void SetActor(Actor actor)
  {
    ActorId id = actor.ToActorId();
    string key = GetActorKey(id);
    _cache.Set(key, actor, _settings.ActorLifetime);
  }
  private static string GetActorKey(ActorId id) => $"Actor.Id={id}";
}
