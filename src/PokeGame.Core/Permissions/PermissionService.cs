using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Permissions;

public interface IPermissionService
{
  Task CheckAsync(string action, CancellationToken cancellationToken = default);
  Task CheckAsync(string action, object? resource, CancellationToken cancellationToken = default);
}

internal class PermissionService : IPermissionService
{
  public static void Register(IServiceCollection services)
  {
    services.AddSingleton(serviceProvider => PermissionSettings.Initialize(serviceProvider.GetRequiredService<IConfiguration>()));
    services.AddTransient<IPermissionService, PermissionService>();
  }

  private readonly IContext _context;
  private readonly PermissionSettings _settings;
  private readonly IWorldQuerier _worldQuerier;

  public PermissionService(IContext context, PermissionSettings settings, IWorldQuerier worldQuerier)
  {
    _context = context;
    _settings = settings;
    _worldQuerier = worldQuerier;
  }

  public async Task CheckAsync(string action, CancellationToken cancellationToken)
  {
    await CheckAsync(action, resource: null, cancellationToken);
  }
  public async Task CheckAsync(string action, object? resource, CancellationToken cancellationToken)
  {
    bool isAllowed = false;

    Entity? entity = null;
    if (resource is null)
    {
      isAllowed = await IsAllowedAsync(action, cancellationToken);
    }
    else if (resource is World world)
    {
      entity = world.GetEntity();
      isAllowed = IsAllowed(action, world);
    }
    else if (resource is IEntityProvider provider)
    {
      entity = provider.GetEntity();
      isAllowed = IsAllowed(action, entity);
    }

    if (!isAllowed)
    {
      throw new PermissionDeniedException(_context.ActorId, action, entity);
    }
  }

  private async Task<bool> IsAllowedAsync(string action, CancellationToken cancellationToken)
  {
    if (action == Actions.CreateWorld)
    {
      int count = await _worldQuerier.CountAsync(cancellationToken);
      return count < _settings.WorldLimit;
    }
    return false;
  }

  private bool IsAllowed(string action, World _) => action == Actions.Update && _context.IsWorldOwner;

  private bool IsAllowed(string action, Entity entity) => action == Actions.Update && entity.WorldId == _context.TryGetWorldId();
}
