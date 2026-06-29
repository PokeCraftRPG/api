using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds.Events;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Worlds.Commands;

internal record UpdateWorldCommand(Guid Id, UpdateWorldPayload Payload) : ICommand<WorldModel?>;

internal class UpdateWorldCommandHandler : ICommandHandler<UpdateWorldCommand, WorldModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IWorldRepository _worldRepository;

  public UpdateWorldCommandHandler(IContext context, IPermissionService permissionService, IWorldRepository worldRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _worldRepository = worldRepository;
  }

  public async Task<WorldModel?> HandleAsync(UpdateWorldCommand command, CancellationToken cancellationToken)
  {
    UpdateWorldPayload payload = command.Payload;
    payload.Validate();

    World? world = await _worldRepository.LoadAsync(command.Id, cancellationToken);
    if (world is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, world, cancellationToken);

    WorldUpdated record = world.Update(
      string.IsNullOrWhiteSpace(payload.Key) ? world.Key : payload.Key,
      payload.Name is null ? world.Name : payload.Name.Value,
      payload.Description is null ? world.Description : payload.Description.Value,
      _context.UserId);
    _worldRepository.Update(world, record);

    await _worldRepository.EnsureUnicityAsync(world, cancellationToken);

    await _context.SaveChangesAsync(cancellationToken);

    return await _worldRepository.ReadAsync(world, cancellationToken);
  }
}
