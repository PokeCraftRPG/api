using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Rosters.Models;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Rosters.Commands;

internal record DeleteTagCommand(Guid TrainerId, Guid TagId) : ICommand<TagDto?>;

internal class DeleteTagCommandHandler : ICommandHandler<DeleteTagCommand, TagDto?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IRosterManager _rosterManager;
  private readonly IRosterQuerier _rosterQuerier;
  private readonly IRosterRepository _rosterRepository;

  public DeleteTagCommandHandler(
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

  public async Task<TagDto?> HandleAsync(DeleteTagCommand command, CancellationToken cancellationToken)
  {
    TrainerId trainerId = new(_context.WorldId, command.TrainerId);
    Roster roster = await _rosterManager.FindAsync(trainerId, nameof(command.TrainerId), cancellationToken);
    await _permissionService.CheckAsync(Actions.ManageTags, roster, cancellationToken);

    if (!roster.HasTag(command.TagId))
    {
      return null;
    }
    TagDto dto = await _rosterQuerier.ReadTagAsync(roster, command.TagId, cancellationToken);

    roster.RemoveTag(command.TagId, _context.ActorId);

    await _rosterRepository.SaveAsync(roster, cancellationToken);

    return dto;
  }
}
