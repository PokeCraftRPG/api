using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Rosters.Models;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Rosters.Commands;

internal record CreateOrReplaceTagCommand(Guid TrainerId, CreateOrReplaceTagPayload Payload, Guid? TagId) : ICommand<CreateOrReplaceTagResult>;

internal class CreateOrReplaceTagCommandHandler : ICommandHandler<CreateOrReplaceTagCommand, CreateOrReplaceTagResult>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IRosterManager _rosterManager;
  private readonly IRosterQuerier _rosterQuerier;
  private readonly IRosterRepository _rosterRepository;

  public CreateOrReplaceTagCommandHandler(
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

  public async Task<CreateOrReplaceTagResult> HandleAsync(CreateOrReplaceTagCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceTagPayload payload = command.Payload;
    payload.Validate();

    TrainerId trainerId = new(_context.WorldId, command.TrainerId);
    Roster roster = await _rosterManager.FindAsync(trainerId, nameof(command.TrainerId), cancellationToken);
    await _permissionService.CheckAsync(Actions.ManageTags, roster, cancellationToken);

    Guid tagId = command.TagId ?? Guid.NewGuid();
    bool created = !roster.HasTag(tagId);

    Name name = new(payload.Name);
    Color? color = payload.Color is null ? null : Color.From(payload.Color);
    Tag tag = new(name, color);
    roster.SetTag(tagId, tag, _context.ActorId);

    await _rosterRepository.SaveAsync(roster, cancellationToken);

    TagDto dto = await _rosterQuerier.ReadTagAsync(roster, tagId, cancellationToken);
    return new CreateOrReplaceTagResult(dto, created);
  }
}
