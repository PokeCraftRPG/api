using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Membership.Commands;

internal record TransferOwnershipCommand(Guid UserId) : ICommand<WorldModel>;

internal class TransferOwnershipCommandHandler : ICommandHandler<TransferOwnershipCommand, WorldModel>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IWorldQuerier _worldQuerier;
  private readonly IWorldRepository _worldRepository;

  public TransferOwnershipCommandHandler(IContext context, IPermissionService permissionService, IWorldQuerier worldQuerier, IWorldRepository worldRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _worldQuerier = worldQuerier;
    _worldRepository = worldRepository;
  }

  public async Task<WorldModel> HandleAsync(TransferOwnershipCommand command, CancellationToken cancellationToken)
  {
    await _permissionService.CheckAsync(Actions.TransferOwnership, cancellationToken);

    World world = await _worldRepository.LoadAsync(_context.WorldId, cancellationToken)
      ?? throw new InvalidOperationException($"The world 'Id={_context.WorldId}' was not loaded.");

    UserId? memberId = world.FindMember(command.UserId);
    if (memberId.HasValue)
    {
      world.TransferOwnership(memberId.Value, _context.UserId);
    }
    else
    {
      throw new MemberNotFoundException(world.Id, command.UserId, nameof(command.UserId));
    }

    await _worldRepository.SaveAsync(world, cancellationToken);

    return await _worldQuerier.ReadAsync(world, cancellationToken);
  }
}
