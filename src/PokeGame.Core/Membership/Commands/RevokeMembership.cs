using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Membership.Commands;

internal record RevokeMembershipCommand(Guid UserId) : ICommand<WorldModel>;

internal class RevokeMembershipCommandHandler : ICommandHandler<RevokeMembershipCommand, WorldModel>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IWorldQuerier _worldQuerier;
  private readonly IWorldRepository _worldRepository;

  public RevokeMembershipCommandHandler(IContext context, IPermissionService permissionService, IWorldQuerier worldQuerier, IWorldRepository worldRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _worldQuerier = worldQuerier;
    _worldRepository = worldRepository;
  }

  public async Task<WorldModel> HandleAsync(RevokeMembershipCommand command, CancellationToken cancellationToken)
  {
    WorldId worldId = _context.WorldId;
    World world = await _worldRepository.LoadAsync(worldId, cancellationToken) ?? throw new InvalidOperationException($"The world 'Id={worldId}' was not loaded.");
    await _permissionService.CheckAsync(Actions.RevokeMembership, world, cancellationToken);

    UserId? memberId = world.FindMember(command.UserId);
    if (memberId.HasValue)
    {
      world.RevokeMembership(memberId.Value, _context.UserId);
    }

    await _worldRepository.SaveAsync(world, cancellationToken);

    return await _worldQuerier.ReadAsync(world, cancellationToken);
  }
}
