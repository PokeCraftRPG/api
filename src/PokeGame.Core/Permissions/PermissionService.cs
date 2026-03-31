using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Permissions;

public interface IPermissionService
{
  Task CheckAsync(string action, CancellationToken cancellationToken = default);
  Task CheckAsync(string action, IEntityProvider? resource, CancellationToken cancellationToken = default);
}

internal class PermissionService : IPermissionService
{
  private readonly HashSet<string> _createActions =
  [
    Actions.CreateAbility,
    Actions.CreateForm,
    Actions.CreateMove,
    Actions.CreateRegion,
    Actions.CreateSpecies,
    Actions.CreateTrainer,
    Actions.CreateVariety
  ];

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
  public async Task CheckAsync(string action, IEntityProvider? resource, CancellationToken cancellationToken)
  {
    bool isAllowed;
    Entity? entity = resource?.GetEntity();
    if (entity is null)
    {
      WorldModel? world = _context.GetWorld();
      if (world is not null)
      {
        entity = new Entity(World.EntityKind, world.Id);
      }
      isAllowed = await IsAllowedAsync(action, cancellationToken);
    }
    else if (resource is World world)
    {
      isAllowed = world.OwnerId == _context.UserId;
    }
    else
    {
      isAllowed = entity.WorldId == _context.WorldId && _context.IsWorldOwner;
    }

    if (!isAllowed)
    {
      throw new PermissionDeniedException(_context.GetUserId(), action, entity);
    }
  }

  private async Task<bool> IsAllowedAsync(string action, CancellationToken cancellationToken)
  {
    if (action == Actions.CreateWorld)
    {
      int count = await _worldQuerier.CountAsync(cancellationToken);
      return count < _settings.WorldLimit;
    }
    else if (_createActions.Contains(action))
    {
      return _context.IsWorldOwner;
    }
    return false;
  }
}
