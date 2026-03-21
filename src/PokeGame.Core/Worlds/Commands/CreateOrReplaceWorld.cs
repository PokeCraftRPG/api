using Logitar.CQRS;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Worlds.Commands;

internal record CreateOrReplaceWorldCommand(CreateOrReplaceWorldPayload Payload, Guid? Id) : ICommand<CreateOrReplaceWorldResult>;

internal class CreateOrReplaceWorldCommandHandler : ICommandHandler<CreateOrReplaceWorldCommand, CreateOrReplaceWorldResult>
{
  private readonly IContext _context;
  private readonly IWorldRepository _worldRepository;

  public CreateOrReplaceWorldCommandHandler(IContext context, IWorldRepository worldRepository)
  {
    _context = context;
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
    Slug slug = new(payload.Slug);

    bool created = false;
    if (world is null)
    {
      // TODO(fpion): Permissions

      world = new World(userId, slug, worldId);
      created = true;
    }
    else
    {
      // TODO(fpion): Permissions

      world.Slug = slug;
    }

    world.Name = Name.TryCreate(payload.Name);
    world.Description = Description.TryCreate(payload.Description);

    world.Update(userId);

    await _worldRepository.SaveAsync(world, cancellationToken); // TODO(fpion): Storage; check for slug conflict

    return new CreateOrReplaceWorldResult(created);
  }
}
