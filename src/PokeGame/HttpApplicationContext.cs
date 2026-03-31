using Krakenar.Contracts;
using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Users;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Actors;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;
using PokeGame.Extensions;

namespace PokeGame;

internal class HttpApplicationContext : IContext
{
  private readonly IHttpContextAccessor _httpContextAccessor;
  private HttpContext Context => _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("The HttpContext is required.");

  public HttpApplicationContext(IHttpContextAccessor httpContextAccessor)
  {
    _httpContextAccessor = httpContextAccessor;
  }

  public UserId UserId => GetUserId() ?? throw new InvalidOperationException("No user was found in the context.");

  public WorldId WorldId
  {
    get
    {
      WorldModel world = GetWorld() ?? throw new InvalidOperationException("No world was found in the context.");
      return new WorldId(world.Id);
    }
  }
  public Guid WorldUid => WorldId.ToGuid();

  public bool IsWorldOwner
  {
    get
    {
      User? user = Context.GetUser();
      WorldModel? world = Context.GetWorld();
      return user is not null && world is not null && world.Owner.Equals(new Actor(user));
    }
  }

  public IReadOnlyCollection<CustomAttribute> GetSessionCustomAttributes() => Context.GetSessionCustomAttributes();

  public UserId? GetUserId()
  {
    User? user = Context.GetUser();
    if (user is null)
    {
      return null;
    }

    Actor actor = new(user);
    ActorId actorId = actor.GetActorId();
    return new UserId(actorId);
  }

  public WorldModel? GetWorld() => Context.GetWorld();
}
