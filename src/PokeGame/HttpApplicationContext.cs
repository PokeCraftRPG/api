using Krakenar.Contracts;
using Krakenar.Contracts.Actors;
using Krakenar.Contracts.ApiKeys;
using Krakenar.Contracts.Users;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Actors;
using PokeGame.Core.Identity;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;
using PokeGame.Extensions;

namespace PokeGame;

internal class HttpApplicationContext : IContext
{
  private readonly IHttpContextAccessor _httpContextAccessor;
  private HttpContext Context => _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("The HttpContext is required.");

  public ActorId? ActorId
  {
    get
    {
      User? user = Context.GetUser();
      if (user is not null)
      {
        Actor actor = new(user);
        return actor.ToActorId();
      }

      ApiKey? apiKey = Context.GetApiKey();
      if (apiKey is not null)
      {
        Actor actor = new(apiKey);
        return actor.ToActorId();
      }

      return null;
    }
  }
  public UserId UserId
  {
    get
    {
      User user = Context.GetUser() ?? throw new InvalidOperationException("An authenticated user is required.");
      return new UserId(user);
    }
  }

  public WorldId WorldId => TryGetWorldId() ?? throw new InvalidOperationException("A world is required.");
  public bool IsWorldOwner
  {
    get
    {
      User? user = Context.GetUser();
      WorldModel? world = Context.GetWorld();
      return world is not null && user is not null && world.Owner.ToActorId() == new Actor(user).ToActorId();
    }
  }

  public HttpApplicationContext(IHttpContextAccessor httpContextAccessor)
  {
    _httpContextAccessor = httpContextAccessor;
  }

  public IReadOnlyCollection<CustomAttribute> GetSessionCustomAttributes() => Context.GetSessionCustomAttributes();

  public WorldId? TryGetWorldId()
  {
    WorldModel? world = Context.GetWorld();
    return world is null ? null : new WorldId(world.Id);
  }
}
