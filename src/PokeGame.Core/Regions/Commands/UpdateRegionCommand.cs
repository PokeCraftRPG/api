using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions.Events;
using PokeGame.Core.Regions.Models;

namespace PokeGame.Core.Regions.Commands;

internal record UpdateRegionCommand(Guid Id, UpdateRegionPayload Payload) : ICommand<RegionModel?>;

internal class UpdateRegionCommandHandler : ICommandHandler<UpdateRegionCommand, RegionModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IRegionRepository _regionRepository;

  public UpdateRegionCommandHandler(IContext context, IPermissionService permissionService, IRegionRepository regionRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _regionRepository = regionRepository;
  }

  public async Task<RegionModel?> HandleAsync(UpdateRegionCommand command, CancellationToken cancellationToken)
  {
    UpdateRegionPayload payload = command.Payload;
    payload.Validate();

    Region? region = await _regionRepository.LoadAsync(command.Id, cancellationToken);
    if (region is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, region, cancellationToken);

    RegionUpdated record = region.Update(
      string.IsNullOrWhiteSpace(payload.Key) ? region.Key : payload.Key,
      payload.Name is null ? region.Name : payload.Name.Value,
      payload.Description is null ? region.Description : payload.Description.Value,
      _context.UserId);
    _regionRepository.Update(region, record);

    await _regionRepository.EnsureUnicityAsync(region, cancellationToken);

    await _context.SaveChangesAsync(cancellationToken);

    return await _regionRepository.ReadAsync(region, cancellationToken);
  }
}
