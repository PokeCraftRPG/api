using Logitar.CQRS;
using PokeGame.Core.Identity;
using PokeGame.Core.Permissions;
using PokeGame.Core.Trainers;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership.Commands;

internal record LeaveMembershipCommand(Guid WorldId) : ICommand<bool>;

internal class LeaveMembershipCommandHandler : ICommandHandler<LeaveMembershipCommand, bool>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly ITrainerManager _trainerManager;
  private readonly IWorldRepository _worldRepository;

  public LeaveMembershipCommandHandler(
    IContext context,
    IPermissionService permissionService,
    ITrainerManager trainerManager,
    IWorldRepository worldRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _trainerManager = trainerManager;
    _worldRepository = worldRepository;
  }

  public async Task<bool> HandleAsync(LeaveMembershipCommand command, CancellationToken cancellationToken)
  {
    WorldId worldId = new(command.WorldId);
    World? world = await _worldRepository.LoadAsync(worldId, cancellationToken);
    if (world is null)
    {
      return false;
    }
    await _permissionService.CheckAsync(Actions.LeaveMembership, world, cancellationToken);

    UserId memberId = _context.UserId;
    world.LeaveMembership(memberId, _context.ActorId);

    await _trainerManager.UnassignMemberAsync(memberId, cancellationToken);
    await _worldRepository.SaveAsync(world, cancellationToken);

    return true;
  }
}
