using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds.Events;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Worlds.Commands;

internal record CreateOrReplaceWorldCommand(CreateOrReplaceWorldPayload Payload, Guid? Id) : ICommand<CreateOrReplaceWorldResult>;

internal class CreateOrReplaceWorldCommandHandler : ICommandHandler<CreateOrReplaceWorldCommand, CreateOrReplaceWorldResult>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IWorldRepository _worldRepository;

  public CreateOrReplaceWorldCommandHandler(IContext context, IPermissionService permissionService, IWorldRepository worldRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _worldRepository = worldRepository;
  }

  public async Task<CreateOrReplaceWorldResult> HandleAsync(CreateOrReplaceWorldCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceWorldPayload payload = command.Payload;
    payload.Validate();

    World? world = null;
    if (command.Id.HasValue)
    {
      world = await _worldRepository.LoadAsync(command.Id.Value, cancellationToken);
    }

    bool created = false;
    if (world is null)
    {
      await _permissionService.CheckAsync(Actions.CreateWorld, cancellationToken);

      world = new World(_context.UserId, payload.Key, command.Id, payload.Name, payload.Description);
      _worldRepository.Add(world);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, world, cancellationToken);

      WorldUpdated record = world.Update(payload.Key, payload.Name, payload.Description, _context.UserId);
      _worldRepository.Update(world, record);
    }

    await _worldRepository.EnsureUnicityAsync(world, cancellationToken);

    await _context.SaveChangesAsync(cancellationToken);

    WorldModel model = await _worldRepository.ReadAsync(world, cancellationToken);
    return new CreateOrReplaceWorldResult(model, created);
  }
}
