using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Rosters.Models;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Rosters.Commands;

internal record UpdateTagCommand(Guid TrainerId, Guid TagId, UpdateTagPayload Payload) : ICommand<TagDto?>;

internal class UpdateTagCommandHandler : ICommandHandler<UpdateTagCommand, TagDto?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IRosterManager _rosterManager;
  private readonly IRosterQuerier _rosterQuerier;
  private readonly IRosterRepository _rosterRepository;

  public UpdateTagCommandHandler(
    IContext context,
    IPermissionService permissionService,
    IRosterManager rosterManager,
    IRosterQuerier rosterQuerier,
    IRosterRepository rosterRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _rosterManager = rosterManager;
    _rosterQuerier = rosterQuerier;
    _rosterRepository = rosterRepository;
  }

  public async Task<TagDto?> HandleAsync(UpdateTagCommand command, CancellationToken cancellationToken)
  {
    UpdateTagPayload payload = command.Payload;
    payload.Validate();

    TrainerId trainerId = new(_context.WorldId, command.TrainerId);
    Roster roster = await _rosterManager.FindAsync(trainerId, nameof(command.TrainerId), cancellationToken);
    await _permissionService.CheckAsync(Actions.ManageTags, roster, cancellationToken);

    Tag? tag = roster.TryGetTag(command.TagId);
    if (tag is null)
    {
      return null;
    }

    tag = new Tag(
      string.IsNullOrWhiteSpace(payload.Name) ? tag.Name : new Name(payload.Name),
      payload.Color is null ? tag.Color : (payload.Color.Value is null ? null : Color.From(payload.Color.Value)));

    roster.SetTag(command.TagId, tag, _context.ActorId);

    await _rosterRepository.SaveAsync(roster, cancellationToken);

    return await _rosterQuerier.ReadTagAsync(roster, command.TagId, cancellationToken);
  }
}
