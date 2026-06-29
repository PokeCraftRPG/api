using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions.Events;
using PokeGame.Core.Regions.Models;

namespace PokeGame.Core.Regions.Commands;

internal record CreateOrReplaceRegionCommand(CreateOrReplaceRegionPayload Payload, Guid? Id) : ICommand<CreateOrReplaceRegionResult>;

internal class CreateOrReplaceRegionCommandHandler : ICommandHandler<CreateOrReplaceRegionCommand, CreateOrReplaceRegionResult>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IRegionRepository _regionRepository;

  public CreateOrReplaceRegionCommandHandler(IContext context, IPermissionService permissionService, IRegionRepository regionRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _regionRepository = regionRepository;
  }

  public async Task<CreateOrReplaceRegionResult> HandleAsync(CreateOrReplaceRegionCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceRegionPayload payload = command.Payload;
    payload.Validate();

    Region? region = null;
    if (command.Id.HasValue)
    {
      region = await _regionRepository.LoadAsync(command.Id.Value, cancellationToken);
    }

    bool created = false;
    if (region is null)
    {
      await _permissionService.CheckAsync(Actions.CreateRegion, cancellationToken);

      region = new Region(_context.WorldId, payload.Key, _context.UserId, command.Id, payload.Name, payload.Description);
      _regionRepository.Add(region);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, region, cancellationToken);

      RegionUpdated record = region.Update(payload.Key, payload.Name, payload.Description, _context.UserId);
      _regionRepository.Update(region, record);
    }

    await _regionRepository.EnsureUnicityAsync(region, cancellationToken);

    await _context.SaveChangesAsync(cancellationToken);

    RegionModel model = await _regionRepository.ReadAsync(region, cancellationToken);
    return new CreateOrReplaceRegionResult(model, created);
  }
}
