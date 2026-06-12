using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Worlds.Commands;

internal record CreateOrReplaceWorldCommand(CreateOrReplaceWorldPayload Payload, Guid? Id) : ICommand<CreateOrReplaceWorldResult>;

internal class CreateOrReplaceWorldCommandHandler : ICommandHandler<CreateOrReplaceWorldCommand, CreateOrReplaceWorldResult>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IWorldManager _worldManager;
  private readonly IWorldQuerier _worldQuerier;
  private readonly IWorldRepository _worldRepository;

  public CreateOrReplaceWorldCommandHandler(
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

  public async Task<CreateOrReplaceWorldResult> HandleAsync(CreateOrReplaceWorldCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceWorldPayload payload = command.Payload;
    payload.Validate();

    WorldId worldId = WorldId.NewId();
    World? world = null;
    if (command.Id.HasValue)
    {
      worldId = new WorldId(command.Id.Value);
      world = await _worldRepository.LoadAsync(worldId, cancellationToken);
    }

    Slug key = new(payload.Key);
    ActorId? actorId = _context.ActorId;

    bool created = false;
    if (world is null)
    {
      await _permissionService.CheckAsync(Actions.CreateWorld, cancellationToken);

      world = new World(_context.UserId, key, worldId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, world, cancellationToken);

      world.SetKey(key, actorId);
    }

    world.Rename(Name.TryCreate(payload.Name), actorId);
    world.Describe(Description.TryCreate(payload.Description), actorId);

    await _worldManager.EnsureUnicityAsync(world, cancellationToken);
    await _worldRepository.SaveAsync(world, cancellationToken);

    WorldModel model = await _worldQuerier.ReadAsync(world, cancellationToken);
    return new CreateOrReplaceWorldResult(model, created);
  }
}
