using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Varieties.Commands;

internal record RemoveVarietyMoveCommand(Guid VarietyId, Guid Id) : ICommand<VarietyDto?>;

internal class RemoveVarietyMoveCommandHandler : ICommandHandler<RemoveVarietyMoveCommand, VarietyDto?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IVarietyQuerier _varietyQuerier;
  private readonly IVarietyRepository _varietyRepository;

  public RemoveVarietyMoveCommandHandler(
    IContext context,
    IPermissionService permissionService,
    IVarietyQuerier varietyQuerier,
    IVarietyRepository varietyRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _varietyQuerier = varietyQuerier;
    _varietyRepository = varietyRepository;
  }

  public async Task<VarietyDto?> HandleAsync(RemoveVarietyMoveCommand command, CancellationToken cancellationToken)
  {
    VarietyId varietyId = new(_context.WorldId, command.VarietyId);
    Variety? variety = await _varietyRepository.LoadAsync(varietyId, cancellationToken);
    if (variety is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, variety, cancellationToken);

    variety.RemoveMove(command.Id, _context.ActorId);

    await _varietyRepository.SaveAsync(variety, cancellationToken);

    return await _varietyQuerier.ReadAsync(variety, cancellationToken);
  }
}
