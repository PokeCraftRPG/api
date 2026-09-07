using Logitar.CQRS;
using PokeGame.Core.Caching;
using PokeGame.Core.Identity;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Membership.Commands;

internal record TransferOwnershipCommand(Guid UserId) : ICommand<WorldDto>;

internal class TransferOwnershipCommandHandler : ICommandHandler<TransferOwnershipCommand, WorldDto>
{
  private readonly ICacheService _cacheService;
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IWorldQuerier _worldQuerier;
  private readonly IWorldRepository _worldRepository;

  public TransferOwnershipCommandHandler(
    ICacheService cacheService,
    IContext context,
    IPermissionService permissionService,
    IWorldQuerier worldQuerier,
    IWorldRepository worldRepository)
  {
    _cacheService = cacheService;
    _context = context;
    _permissionService = permissionService;
    _worldQuerier = worldQuerier;
    _worldRepository = worldRepository;
  }

  public async Task<WorldDto> HandleAsync(TransferOwnershipCommand command, CancellationToken cancellationToken)
  {
    World world = await _worldRepository.LoadAsync(_context.WorldId, cancellationToken)
      ?? throw new InvalidOperationException($"The world 'Id={_context.WorldId}' was not loaded.");
    await _permissionService.CheckAsync(Actions.TransferOwnership, world, cancellationToken);

    UserId userId = new(command.UserId, _cacheService.Realm?.Id);
    world.TransferOwnership(userId, _context.ActorId);

    await _worldRepository.SaveAsync(world, cancellationToken);

    return await _worldQuerier.ReadAsync(world, cancellationToken);
  }
}
