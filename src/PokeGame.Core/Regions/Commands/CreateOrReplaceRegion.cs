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

    ActorId? actorId = _context.ActorId;
    Key key = new(payload.Key);

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

    region.SetDetails(Name.TryCreate(payload.Name), Summary.TryCreate(payload.Summary), Content.TryCreate(payload.Content), actorId);

    await _regionManager.EnsureUnicityAsync(region, cancellationToken);
    await _regionRepository.SaveAsync(region, cancellationToken);

    RegionDto dto = await _regionQuerier.ReadAsync(region, cancellationToken);
    return new CreateOrReplaceRegionResult(dto, created);
  }
}
