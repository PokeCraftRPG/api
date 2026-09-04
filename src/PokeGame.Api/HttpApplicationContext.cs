using Krakenar.Contracts;
using Krakenar.Contracts.Actors;
using Krakenar.Contracts.ApiKeys;
using Krakenar.Contracts.Users;
using Logitar.EventSourcing;
using PokeGame.Api.Extensions;
using PokeGame.Core;
using PokeGame.Core.Actors;
using PokeGame.Core.Identity;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Api;

internal class HttpApplicationContext : IContext
{
  private readonly IHttpContextAccessor _httpContextAccessor;
  private HttpContext Context => _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("The HttpContext is required.");

  public HttpApplicationContext(IHttpContextAccessor httpContextAccessor)
  {
    _httpContextAccessor = httpContextAccessor;
  }

  public ActorId? ActorId
  {
    get
    {
      User? user = Context.GetUser();
      if (user is not null)
      {
        return new Actor(user).ToActorId();
      }

      ApiKey? apiKey = Context.GetApiKey();
      if (apiKey is not null)
      {
        return new Actor(apiKey).ToActorId();
      }

      return null;
    }
  }
  public UserId UserId => TryGetUserId() ?? throw new InvalidOperationException("An authenticated user is required.");
  public WorldId WorldId => TryGetWorldId() ?? throw new InvalidOperationException("A world is required.");
  public bool IsWorldOwner
  {
    get
    {
      User? user = Context.GetUser();
      WorldDto? world = Context.GetWorld();
      return user is not null && world is not null && world.Owner.Equals(new Actor(user));
    }
  }

  public IReadOnlyCollection<CustomAttribute> GetSessionCustomAttributes() => Context.GetSessionCustomAttributes();

  public Guid? TryGetSessionId() => Context.GetSession()?.Id;
  public UserId? TryGetUserId()
  {
    User? user = Context.GetUser();
    return user is null ? null : new UserId(new Actor(user).ToActorId());
  }
  public WorldId? TryGetWorldId()
  {
    WorldDto? world = Context.GetWorld();
    return world is null ? null : new WorldId(world.Id);
  }
}
