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

  public UserId UserId
  {
    get
    {
      User user = Context.GetUser() ?? throw new InvalidOperationException("No user was found in the context.");
      Actor actor = new(user);
      ActorId actorId = actor.GetActorId();
      return new UserId(actorId);
    }
  }

  public WorldId WorldId
  {
    get
    {
      WorldModel world = Context.GetWorld() ?? throw new InvalidOperationException("No world was found in the context.");
      return new WorldId(world.Id);
    }
  }
  public Guid WorldUid => WorldId.ToGuid();

  public IReadOnlyCollection<CustomAttribute> GetSessionCustomAttributes() => Context.GetSessionCustomAttributes();
}
