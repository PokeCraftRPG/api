using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions.Models;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Regions.Commands;

internal record UpdateRegionCommand(Guid Id, UpdateRegionPayload Payload) : ICommand<RegionModel?>;

internal class UpdateRegionCommandHandler : ICommandHandler<UpdateRegionCommand, RegionModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IRegionQuerier _regionQuerier;
  private readonly IRegionRepository _regionRepository;
  private readonly IStorageService _storageService;

  public UpdateRegionCommandHandler(
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

    UserId userId = _context.UserId;

    if (!string.IsNullOrWhiteSpace(payload.Key))
    {
      Slug key = new(payload.Key);
      region.SetKey(key, userId);
    }
    if (payload.Name is not null)
    {
      region.Name = Name.TryCreate(payload.Name.Value);
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

    region.Update(userId);

    await _regionQuerier.EnsureUnicityAsync(region, cancellationToken);

    await _storageService.ExecuteWithQuotaAsync(
      region,
      async () => await _regionRepository.SaveAsync(region, cancellationToken),
      cancellationToken);

    return await _regionQuerier.ReadAsync(region, cancellationToken);
  }
}
