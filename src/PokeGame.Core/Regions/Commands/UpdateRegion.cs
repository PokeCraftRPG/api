using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions.Models;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Regions.Commands;

internal record UpdateRegionCommand(Guid Id, UpdateRegionPayload Payload) : ICommand<RegionModel?>;

internal class UpdateRegionCommandHandler : ICommandHandler<UpdateRegionCommand, RegionModel?>
{
  private readonly IRegionQuerier _regionQuerier;
  private readonly IRegionRepository _regionRepository;
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IStorageService _storageService;

  public UpdateRegionCommandHandler(
    IRegionQuerier regionQuerier,
    IRegionRepository regionRepository,
    IContext context,
    IPermissionService permissionService,
    IStorageService storageService)
  {
    _regionQuerier = regionQuerier;
    _regionRepository = regionRepository;
    _context = context;
    _permissionService = permissionService;
    _storageService = storageService;
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

    if (!string.IsNullOrWhiteSpace(payload.Name))
    {
      region.Name = new Name(payload.Name);
    }
    if (payload.Description is not null)
    {
      region.Description = Description.TryCreate(payload.Description.Value);
    }

    if (payload.Url is not null)
    {
      region.Url = Url.TryCreate(payload.Url.Value);
    }
    if (payload.Notes is not null)
    {
      region.Notes = Notes.TryCreate(payload.Notes.Value);
    }

    region.Update(_context.UserId);

    await _storageService.ExecuteWithQuotaAsync(
      region,
      async () => await _regionRepository.SaveAsync(region, cancellationToken),
      cancellationToken);

    return await _regionQuerier.ReadAsync(region, cancellationToken);
  }
}
