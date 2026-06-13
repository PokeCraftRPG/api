using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions.Models;

namespace PokeGame.Core.Regions.Commands;

internal record UpdateRegionCommand(Guid Id, UpdateRegionPayload Payload) : ICommand<RegionModel?>;

internal class UpdateRegionCommandHandler : ICommandHandler<UpdateRegionCommand, RegionModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IRegionManager _regionManager;
  private readonly IRegionQuerier _regionQuerier;
  private readonly IRegionRepository _regionRepository;

  public UpdateRegionCommandHandler(
    IContext context,
    IPermissionService permissionService,
    IRegionManager regionManager,
    IRegionQuerier regionQuerier,
    IRegionRepository regionRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _regionManager = regionManager;
    _regionQuerier = regionQuerier;
    _regionRepository = regionRepository;
  }

  public async Task<RegionModel?> HandleAsync(UpdateRegionCommand command, CancellationToken cancellationToken)
  {
    UpdateRegionPayload payload = command.Payload;
    payload.Validate();

    RegionId regionId = new(_context.WorldId, command.Id);
    Region? region = await _regionRepository.LoadAsync(regionId, cancellationToken);
    if (region is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, region, cancellationToken);

    ActorId? actorId = _context.ActorId;

    if (!string.IsNullOrWhiteSpace(payload.Key))
    {
      region.SetKey(new Slug(payload.Key), actorId);
    }
    if (payload.Name is not null)
    {
      region.Rename(Name.TryCreate(payload.Name.Value), actorId);
    }
    if (payload.Description is not null)
    {
      region.Describe(Description.TryCreate(payload.Description.Value), actorId);
    }

    await _regionManager.EnsureUnicityAsync(region, cancellationToken);
    await _regionRepository.SaveAsync(region, cancellationToken);

    return await _regionQuerier.FindAsync(region, cancellationToken);
  }
}
