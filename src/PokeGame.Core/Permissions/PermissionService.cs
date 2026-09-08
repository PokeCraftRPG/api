using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Identity;
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
  private readonly IWorldRepository _worldRepository;

  public PermissionService(IContext context, PermissionSettings settings, IWorldQuerier worldQuerier, IWorldRepository worldRepository)
  {
    _context = context;
    _settings = settings;
    _worldQuerier = worldQuerier;
    _worldRepository = worldRepository;
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
      isAllowed = await IsAllowedAsync(action, invitation, cancellationToken);
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
      case Actions.CreateForm:
      case Actions.CreateMove:
      case Actions.CreateRegion:
      case Actions.CreateSpecies:
      case Actions.CreateTrainer:
      case Actions.CreateVariety:
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
      case Actions.InviteMember:
      case Actions.RevokeMembership:
      case Actions.TransferOwnership:
      case Actions.Update:
      case Actions.ViewInvitations:
        return world.OwnerId == _context.TryGetUserId();
      case Actions.LeaveMembership:
        UserId? userId = _context.TryGetUserId();
        return userId.HasValue && world.IsMember(userId.Value);
      default:
        return false;
    }
  }

  private async Task<bool> IsAllowedAsync(string action, MemberInvitation invitation, CancellationToken cancellationToken)
  {
    switch (action)
    {
      case Actions.Accept:
      case Actions.Decline:
        return invitation.UserId == _context.TryGetUserId();
      case Actions.Cancel:
        World? world = await _worldRepository.LoadAsync(invitation.WorldId, cancellationToken);
        return world is not null && world.OwnerId == _context.TryGetUserId();
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
