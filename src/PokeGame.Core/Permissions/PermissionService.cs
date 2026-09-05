using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Membership;
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
    else if (resource is MemberInvitation invitation)
    {
      entity = invitation.GetEntity();
      isAllowed = IsAllowed(action, invitation);
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

  private async Task<bool> IsAllowedAsync(string action, CancellationToken cancellationToken)
  {
    switch (action)
    {
      case Actions.CreateAbility:
      case Actions.CreateMove:
      case Actions.CreateRegion:
      case Actions.CreateSpecies:
      case Actions.CreateVariety:
      case Actions.InviteMember:
      case Actions.Upload:
        return _context.IsWorldOwner;
      case Actions.CreateWorld:
        int count = await _worldQuerier.CountAsync(cancellationToken);
        return count < _settings.WorldLimit;
      default:
        return false;
    }
  }

  private bool IsAllowed(string action, World world)
  {
    switch (action)
    {
      case Actions.Update:
        return world.OwnerId == _context.TryGetUserId();
      default:
        return false;
    }
  }

  private bool IsAllowed(string action, MemberInvitation invitation)
  {
    switch (action)
    {
      case Actions.Accept:
      case Actions.Decline:
        return invitation.UserId == _context.TryGetUserId();
      case Actions.Cancel:
        return _context.IsWorldOwner && invitation.WorldId == _context.TryGetWorldId();
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
