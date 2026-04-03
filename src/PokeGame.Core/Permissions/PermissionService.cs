using PokeGame.Core.Membership;
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
      isAllowed = IsAllowed(action, world);
    }
    else if (resource is MembershipInvitation invitation)
    {
      isAllowed = IsAllowed(action, invitation);
    }
    else
    {
      isAllowed = IsAllowed(action, entity);
    }

    if (!isAllowed)
    {
      throw new PermissionDeniedException(_context.GetUserId(), action, entity);
    }
  }

  private async Task<bool> IsAllowedAsync(string action, CancellationToken cancellationToken) => action switch
  {
    Actions.CreateAbility or Actions.CreateEvolution or Actions.CreateForm or Actions.CreateItem or Actions.CreateMove or Actions.CreatePokemon
      or Actions.CreateRegion or Actions.CreateSpecies or Actions.CreateTrainer or Actions.CreateVariety => _context.IsWorldOwner,
    Actions.CreateWorld => await CanCreateWorldAsync(cancellationToken),
    _ => false,
  };
  private async Task<bool> CanCreateWorldAsync(CancellationToken cancellationToken)
  {
    int count = await _worldQuerier.CountAsync(cancellationToken);
    return count < _settings.WorldLimit;
  }

  private bool IsAllowed(string action, Entity entity) => action switch
  {
    Actions.Update => entity.WorldId == _context.WorldId && _context.IsWorldOwner,
    _ => false,
  };

  private bool IsAllowed(string action, MembershipInvitation invitation) => action switch
  {
    Actions.Accept or Actions.Decline => invitation.InviteeId == _context.UserId,
    Actions.Cancel => invitation.WorldId == _context.WorldId && _context.IsWorldOwner,
    _ => false,
  };

  private bool IsAllowed(string action, World world) => action switch
  {
    Actions.RevokeMembership or Actions.SendMembershipInvitation or Actions.Update => world.OwnerId == _context.UserId,
    _ => false,
  };
}
