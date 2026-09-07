using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership.Commands;

internal record LeaveMembershipCommand : ICommand;

internal class LeaveMembershipCommandHandler : ICommandHandler<LeaveMembershipCommand, Unit>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IWorldRepository _worldRepository;

  public LeaveMembershipCommandHandler(IContext context, IPermissionService permissionService, IWorldRepository worldRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _worldRepository = worldRepository;
  }

  public async Task<Unit> HandleAsync(LeaveMembershipCommand command, CancellationToken cancellationToken)
  {
    World world = await _worldRepository.LoadAsync(_context.WorldId, cancellationToken)
      ?? throw new InvalidOperationException($"The world 'Id={_context.WorldId}' was not loaded.");
    await _permissionService.CheckAsync(Actions.LeaveMember, world, cancellationToken);

    world.LeaveMembership(_context.UserId, _context.ActorId);

    await _worldRepository.SaveAsync(world, cancellationToken);

    return Unit.Value;
  }
}
