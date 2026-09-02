using Microsoft.Extensions.DependencyInjection;

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
    services.AddTransient<IPermissionService, PermissionService>();
  }

  private readonly IContext _context;

  public PermissionService(IContext context)
  {
    _context = context;
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
      isAllowed = IsAllowed(action);
    }
    else if (resource is IEntityProvider provider)
    {
      entity = provider.GetEntity();
      isAllowed = IsAllowed(action, entity);
    }

    if (!isAllowed)
    {
      throw new PermissionDeniedException(_context.ActorId, action, entity, _context.TryGetWorldId());
    }
  }

  private bool IsAllowed(string action)
  {
    switch (action)
    {
      case Actions.CreateRegion:
        return _context.IsWorldOwner;
      default:
        return false;
    }
  }

  private bool IsAllowed(string action, Entity entity)
  {
    switch (action)
    {
      case Actions.Update:
        return _context.IsWorldOwner && entity.WorldId == _context.TryGetWorldId();
      default:
        return false;
    }
  }
}
