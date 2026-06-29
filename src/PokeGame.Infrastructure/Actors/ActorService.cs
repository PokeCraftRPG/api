using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Users;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Caching;
using PokeGame.Core.Identity;

namespace PokeGame.Infrastructure.Actors;

public interface IActorService
{
  Task<IReadOnlyDictionary<Guid, Actor>> FindAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}

internal class ActorService : IActorService
{
  public static void Register(IServiceCollection services)
  {
    services.AddSingleton<IActorService, ActorService>();
  }

  private readonly ICacheService _cacheService;
  private readonly IUserGateway _userGateway;

  public ActorService(ICacheService cacheService, IUserGateway userGateway)
  {
    _cacheService = cacheService;
    _userGateway = userGateway;
  }

  public async Task<IReadOnlyDictionary<Guid, Actor>> FindAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
  {
    int capacity = ids.Count();
    Dictionary<Guid, User> foundUsers = new(capacity);

    if (capacity > 0)
    {
      HashSet<Guid> missingIds = new(capacity);
      foreach (Guid id in ids)
      {
        User? user = _cacheService.GetUser(id);
        if (user is null)
        {
          missingIds.Add(id);
        }
        else
        {
          foundUsers[id] = user;
        }
      }

      if (missingIds.Count > 0)
      {
        IReadOnlyCollection<User> users = await _userGateway.FindAsync(missingIds, cancellationToken);
        foreach (User user in users)
        {
          foundUsers[user.Id] = user;
        }
      }

      foreach (User user in foundUsers.Values)
      {
        _cacheService.SetUser(user);
      }
    }

    return foundUsers.ToDictionary(x => x.Key, x => new Actor(x.Value)).AsReadOnly();
  }
}
