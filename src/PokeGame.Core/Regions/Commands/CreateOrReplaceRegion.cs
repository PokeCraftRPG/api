using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions.Events;
using PokeGame.Core.Regions.Models;
using PokeGame.Core.Storages;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Regions.Commands;

internal record CreateOrReplaceRegionCommand(CreateOrReplaceRegionPayload Payload, Guid? Id) : ICommand<CreateOrReplaceRegionResult>;

internal class CreateOrReplaceRegionCommandHandler : ICommandHandler<CreateOrReplaceRegionCommand, CreateOrReplaceRegionResult>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IRegionQuerier _regionQuerier;
  private readonly IRegionRepository _regionRepository;
  private readonly IStorageService _storageService;

  public CreateOrReplaceRegionCommandHandler(
    IContext context,
    IPermissionService permissionService,
    IRegionQuerier regionQuerier,
    IRegionRepository regionRepository,
    IStorageService storageService)
  {
    _context = context;
    _permissionService = permissionService;
    _regionQuerier = regionQuerier;
    _regionRepository = regionRepository;
    _storageService = storageService;
  }

  public async Task<CreateOrReplaceRegionResult> HandleAsync(CreateOrReplaceRegionCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceRegionPayload payload = command.Payload;
    payload.Validate();

    UserId userId = _context.UserId;
    WorldId worldId = _context.WorldId;

    RegionId regionId = RegionId.NewId(worldId);
    Region? region = null;
    if (command.Id.HasValue)
    {
      regionId = new(worldId, command.Id.Value);
      region = await _regionRepository.LoadAsync(regionId, cancellationToken);
    }

    Slug key = new(payload.Key);

    bool created = false;
    if (region is null)
    {
      await _permissionService.CheckAsync(Actions.CreateRegion, cancellationToken);

      region = new(key, userId, regionId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, region, cancellationToken);

      region.SetKey(key, userId);
    }

    region.Name = Name.TryCreate(payload.Name);
    region.Description = Description.TryCreate(payload.Description);

    region.Url = Url.TryCreate(payload.Url);
    region.Notes = Notes.TryCreate(payload.Notes);

    region.Update(userId);

    if (region.Changes.Any(change => change is RegionCreated || change is RegionKeyChanged))
    {
      await _regionQuerier.EnsureUnicityAsync(region, cancellationToken);
    }

    await _storageService.ExecuteWithQuotaAsync(
      region,
      async () => await _regionRepository.SaveAsync(region, cancellationToken),
      cancellationToken);

    RegionModel model = await _regionQuerier.ReadAsync(region, cancellationToken);
    return new CreateOrReplaceRegionResult(model, created);
  }
}
