using Krakenar.Contracts.Users;
using PokeGame.Core;
using PokeGame.Core.Worlds.Models;
using PokeGame.Extensions;
using PokeGame.Infrastructure;

namespace PokeGame;

internal class HttpApplicationContext : IContext
{
  private readonly IHttpContextAccessor _httpContextAccessor;
  private HttpContext Context => _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("The HttpContext is required.");

  public HttpApplicationContext(IHttpContextAccessor httpContextAccessor)
  {
    _httpContextAccessor = httpContextAccessor;
  }

  public Guid UserId => TryGetUserId() ?? throw new InvalidOperationException("An authenticated user is required.");
  public Guid WorldId => TryGetWorldId() ?? throw new InvalidOperationException("A world is required.");

  public bool IsWorldOwner()
  {
    User? user = Context.GetUser();
    WorldModel? world = Context.GetWorld();
    return user is not null && world is not null && world.Owner.Id == user.Id;
  }

  public Guid? TryGetUserId() => Context.GetUser()?.Id;
  public Guid? TryGetWorldId() => Context.GetWorld()?.Id;

  public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
  {
    PokemonContext database = Context.RequestServices.GetRequiredService<PokemonContext>();
    return await database.SaveChangesAsync(cancellationToken);
  }
}
