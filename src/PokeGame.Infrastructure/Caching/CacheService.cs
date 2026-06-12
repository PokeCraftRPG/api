using Krakenar.Contracts.Actors;
using Logitar.EventSourcing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Actors;
using PokeGame.Core.Caching;

namespace PokeGame.Infrastructure.Caching;

internal class CacheService : ICacheService
{
  public static void Register(IServiceCollection services)
  {
    services.AddSingleton(serviceProvider => CachingSettings.Initialize(serviceProvider.GetRequiredService<IConfiguration>()));
    services.AddSingleton<ICacheService, CacheService>();
  }

  private const string RealmIdKey = "RealmId";

  private readonly IMemoryCache _cache;
  private readonly CachingSettings _settings;

  public Guid RealmId
  {
    get => (_cache.TryGetValue(RealmIdKey, out object? value) ? (Guid?)value : null) ?? throw new InvalidOperationException("The realm identifier was not found in the cache.");
    set => _cache.Set(RealmIdKey, value);
  }

  public CacheService(IMemoryCache cache, CachingSettings settings)
  {
    _cache = cache;
    _settings = settings;
  }

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
    string key = GetActorKey(actor.ToActorId());
    _cache.Set(key, actor, _settings.ActorLifetime);
  }
  private static string GetActorKey(ActorId id) => $"Actor.Id={id}";
}
