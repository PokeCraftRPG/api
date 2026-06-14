using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions.Models;

namespace PokeGame.Core.Regions.Commands;

internal record CreateOrReplaceRegionCommand(CreateOrReplaceRegionPayload Payload, Guid? Id) : ICommand<CreateOrReplaceRegionResult>;

internal class CreateOrReplaceRegionCommandHandler : ICommandHandler<CreateOrReplaceRegionCommand, CreateOrReplaceRegionResult>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IRegionManager _regionManager;
  private readonly IRegionQuerier _regionQuerier;
  private readonly IRegionRepository _regionRepository;

  public CreateOrReplaceRegionCommandHandler(
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

  public async Task<CreateOrReplaceRegionResult> HandleAsync(CreateOrReplaceRegionCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceRegionPayload payload = command.Payload;
    payload.Validate();

    RegionId regionId = RegionId.NewId(_context.WorldId);
    Region? region = null;
    if (command.Id.HasValue)
    {
      regionId = new RegionId(regionId.WorldId, command.Id.Value);
      region = await _regionRepository.LoadAsync(regionId, cancellationToken);
    }

    Slug key = new(payload.Key);
    ActorId? actorId = _context.ActorId;

    bool created = false;
    if (region is null)
    {
      await _permissionService.CheckAsync(Actions.CreateRegion, cancellationToken);

      region = new Region(regionId, key, actorId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, region, cancellationToken);

      region.SetKey(key, actorId);
    }

    region.Rename(Name.TryCreate(payload.Name), actorId);
    region.Describe(Description.TryCreate(payload.Description), actorId);

    await _regionManager.EnsureUnicityAsync(region, cancellationToken);
    await _regionRepository.SaveAsync(region, cancellationToken);

    RegionModel model = await _regionQuerier.FindAsync(region, cancellationToken);
    return new CreateOrReplaceRegionResult(model, created);
  }
}
