using Krakenar.Contracts.Users;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Caching;

namespace PokeGame.Infrastructure.Caching;

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

  public User? GetUser(Guid id)
  {
    string key = GetUserKey(id);
    return _cache.TryGetValue(key, out object? value) ? (User?)value : null;
  }
  public void RemoveUser(Guid id)
  {
    string key = GetUserKey(id);
    _cache.Remove(key);
  }
  public void SetUser(User user)
  {
    string key = GetUserKey(user.Id);
    _cache.Set(key, user, _settings.ActorLifetime);
  }
  private static string GetUserKey(Guid id) => $"User.Id:{id}";
}
