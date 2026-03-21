using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Worlds.Commands;

internal record CreateOrReplaceWorldCommand(CreateOrReplaceWorldPayload Payload, Guid? Id) : ICommand<CreateOrReplaceWorldResult>;

internal class CreateOrReplaceWorldCommandHandler : ICommandHandler<CreateOrReplaceWorldCommand, CreateOrReplaceWorldResult>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IStorageService _storageService;
  private readonly IWorldQuerier _worldQuerier;
  private readonly IWorldRepository _worldRepository;

  public CreateOrReplaceWorldCommandHandler(
    IContext context,
    IPermissionService permissionService,
    IStorageService storageService,
    IWorldQuerier worldQuerier,
    IWorldRepository worldRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _storageService = storageService;
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
      worldId = new(command.Id.Value);
      world = await _worldRepository.LoadAsync(worldId, cancellationToken);
    }

    UserId userId = _context.UserId;
    Name name = new(payload.Name);

    bool created = false;
    if (world is null)
    {
      await _permissionService.CheckAsync(Actions.CreateWorld, cancellationToken);

      world = new World(userId, name, worldId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, world, cancellationToken);

      world.Name = name;
    }

    world.Description = Description.TryCreate(payload.Description);

    world.Update(userId);

    await _storageService.ExecuteWithQuotaAsync(
      world,
      async () => await _worldRepository.SaveAsync(world, cancellationToken),
      cancellationToken);

    WorldModel model = await _worldQuerier.ReadAsync(world, cancellationToken);
    return new CreateOrReplaceWorldResult(model, created);
  }
}
