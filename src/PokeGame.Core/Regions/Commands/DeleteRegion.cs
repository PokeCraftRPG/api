using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions.Models;

namespace PokeGame.Core.Regions.Commands;

internal record DeleteRegionCommand(Guid Id) : ICommand<RegionModel?>;

internal class DeleteRegionCommandHandler : ICommandHandler<DeleteRegionCommand, RegionModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IRegionRepository _regionRepository;

  public DeleteRegionCommandHandler(IContext context, IPermissionService permissionService, IRegionRepository regionRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _regionRepository = regionRepository;
  }

  public async Task<RegionModel?> HandleAsync(DeleteRegionCommand command, CancellationToken cancellationToken)
  {
    Region? region = await _regionRepository.LoadAsync(command.Id, cancellationToken);
    if (region is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Delete, region, cancellationToken);

    RegionModel model = await _regionRepository.ReadAsync(region, cancellationToken);

    _regionRepository.Remove(region);

    await _context.SaveChangesAsync(cancellationToken);

    return model;
  }
}
