using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Users;
using Logitar.EventSourcing;
using PokeGame.Core.Actors;
using PokeGame.Core.Caching;
using PokeGame.Core.Identity;

namespace PokeGame.Infrastructure.Actors;

public interface IActorService
{
  Task<IReadOnlyDictionary<ActorId, Actor>> FindAsync(IEnumerable<ActorId> ids, CancellationToken cancellationToken = default);
}

internal class ActorService : IActorService
{
  private readonly ICacheService _cacheService;
  private readonly IUserGateway _userGateway;

  public ActorService(ICacheService cacheService, IUserGateway userGateway)
  {
    _cacheService = cacheService;
    _userGateway = userGateway;
  }

  public async Task<IReadOnlyDictionary<ActorId, Actor>> FindAsync(IEnumerable<ActorId> ids, CancellationToken cancellationToken)
  {
    int capacity = ids.Count();
    Dictionary<ActorId, Actor> actors = new(capacity);
    HashSet<Guid> userIds = new(capacity);

    foreach (ActorId id in ids)
    {
      Actor? actor = _cacheService.GetActor(id);
      if (actor is null)
      {
        actor = id.ToActor();
        if (actor.Type == ActorType.User)
        {
          userIds.Add(actor.Id);
        }
      }
      actors[id] = actor;
    }

    if (userIds.Count > 0)
    {
      IReadOnlyCollection<User> users = await _userGateway.FindAsync(userIds, cancellationToken);
      foreach (User user in users)
      {
        Actor actor = new(user);
        ActorId actorId = actor.GetActorId();
        actors[actorId] = actor;
      }
    }

    foreach (Actor actor in actors.Values)
    {
      _cacheService.SetActor(actor);
    }

    return actors.AsReadOnly();
  }
}
