using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Worlds.Commands;

internal record DeleteWorldCommand(Guid Id) : ICommand<WorldModel?>;

internal class DeleteWorldCommandHandler : ICommandHandler<DeleteWorldCommand, WorldModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IWorldRepository _worldRepository;

  public DeleteWorldCommandHandler(IContext context, IPermissionService permissionService, IWorldRepository worldRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _worldRepository = worldRepository;
  }

  public async Task<WorldModel?> HandleAsync(DeleteWorldCommand command, CancellationToken cancellationToken)
  {
    World? world = await _worldRepository.LoadAsync(command.Id, cancellationToken);
    if (world is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Delete, world, cancellationToken);

    WorldModel model = await _worldRepository.ReadAsync(world, cancellationToken);

    _worldRepository.Remove(world);

    await _context.SaveChangesAsync(cancellationToken);

    return model;
  }
}
