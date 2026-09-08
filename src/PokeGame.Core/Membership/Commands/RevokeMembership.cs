using Logitar.CQRS;
using PokeGame.Core.Caching;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Trainers;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Membership.Commands;

internal record RevokeMembershipCommand(Guid WorldId, RevokeMembershipPayload Payload) : ICommand<WorldDto?>;

internal class RevokeMembershipCommandHandler : ICommandHandler<RevokeMembershipCommand, WorldDto?>
{
  private readonly ICacheService _cacheService;
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly ITrainerManager _trainerManager;
  private readonly IWorldQuerier _worldQuerier;
  private readonly IWorldRepository _worldRepository;

  public RevokeMembershipCommandHandler(
    ICacheService cacheService,
    IContext context,
    IPermissionService permissionService,
    ITrainerManager trainerManager,
    IWorldQuerier worldQuerier,
    IWorldRepository worldRepository)
  {
    _cacheService = cacheService;
    _context = context;
    _permissionService = permissionService;
    _trainerManager = trainerManager;
    _worldQuerier = worldQuerier;
    _worldRepository = worldRepository;
  }

  public async Task<WorldDto?> HandleAsync(RevokeMembershipCommand command, CancellationToken cancellationToken)
  {
    RevokeMembershipPayload payload = command.Payload;

    WorldId worldId = new(command.WorldId);
    World? world = await _worldRepository.LoadAsync(worldId, cancellationToken);
    if (world is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.RevokeMembership, world, cancellationToken);

    UserId memberId = new(payload.UserId, _cacheService.Realm?.Id);
    world.RevokeMembership(memberId, _context.ActorId);

    await _trainerManager.UnassignMemberAsync(memberId, cancellationToken);
    await _worldRepository.SaveAsync(world, cancellationToken);

    return await _worldQuerier.ReadAsync(world, cancellationToken);
  }
}
