using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Worlds.Commands;

internal record UpdateWorldCommand(Guid Id, UpdateWorldPayload Payload) : ICommand<WorldModel?>;

internal class UpdateWorldCommandHandler : ICommandHandler<UpdateWorldCommand, WorldModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IWorldManager _worldManager;
  private readonly IWorldQuerier _worldQuerier;
  private readonly IWorldRepository _worldRepository;

  public UpdateWorldCommandHandler(
    IContext context,
    IPermissionService permissionService,
    IWorldManager worldManager,
    IWorldQuerier worldQuerier,
    IWorldRepository worldRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _worldManager = worldManager;
    _worldQuerier = worldQuerier;
    _worldRepository = worldRepository;
  }

  public async Task<WorldModel?> HandleAsync(UpdateWorldCommand command, CancellationToken cancellationToken)
  {
    UpdateWorldPayload payload = command.Payload;
    payload.Validate();

    WorldId worldId = new(command.Id);
    World? world = await _worldRepository.LoadAsync(worldId, cancellationToken);
    if (world is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, world, cancellationToken);

    ActorId? actorId = _context.ActorId;

    if (!string.IsNullOrWhiteSpace(payload.Key))
    {
      world.SetKey(new Slug(payload.Key), actorId);
    }
    if (payload.Name is not null)
    {
      world.Rename(Name.TryCreate(payload.Name.Value), actorId);
    }
    if (payload.Description is not null)
    {
      world.Describe(Description.TryCreate(payload.Description.Value), actorId);
    }

    await _worldManager.EnsureUnicityAsync(world, cancellationToken);
    await _worldRepository.SaveAsync(world, cancellationToken);

    return await _worldQuerier.ReadAsync(world, cancellationToken);
  }
}
