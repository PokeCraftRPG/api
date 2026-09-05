using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Permissions;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Varieties.Commands;

internal record UpdateVarietyCommand(Guid Id, UpdateVarietyPayload Payload) : ICommand<VarietyDto?>;

internal class UpdateVarietyCommandHandler : ICommandHandler<UpdateVarietyCommand, VarietyDto?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IVarietyManager _varietyManager;
  private readonly IVarietyQuerier _varietyQuerier;
  private readonly IVarietyRepository _varietyRepository;

  public UpdateVarietyCommandHandler(
    IContext context,
    IPermissionService permissionService,
    IVarietyManager varietyManager,
    IVarietyQuerier varietyQuerier,
    IVarietyRepository varietyRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _varietyManager = varietyManager;
    _varietyQuerier = varietyQuerier;
    _varietyRepository = varietyRepository;
  }

  public async Task<VarietyDto?> HandleAsync(UpdateVarietyCommand command, CancellationToken cancellationToken)
  {
    UpdateVarietyPayload payload = command.Payload;
    payload.Validate();

    VarietyId varietyId = new(_context.WorldId, command.Id);
    Variety? variety = await _varietyRepository.LoadAsync(varietyId, cancellationToken);
    if (variety is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, variety, cancellationToken);

    ActorId? actorId = _context.ActorId;

    if (payload.IsDefault.HasValue)
    {
      variety.SetDefault(payload.IsDefault.Value, actorId);
    }

    if (!string.IsNullOrWhiteSpace(payload.Key))
    {
      variety.SetKey(new Key(payload.Key), actorId);
    }

    if (payload.Name is not null || payload.Summary is not null || payload.Content is not null)
    {
      variety.SetDetails(
        payload.Name is null ? variety.Name : Name.TryCreate(payload.Name.Value),
        payload.Summary is null ? variety.Summary : Summary.TryCreate(payload.Summary.Value),
        payload.Content is null ? variety.Content : Content.TryCreate(payload.Content.Value),
        actorId);
    }

    if (payload.CanChangeForm is not null || payload.GenderRatio is not null || payload.Genus is not null)
    {
      variety.SetTraits(
        payload.CanChangeForm is null ? variety.CanChangeForm : payload.CanChangeForm.Value,
        payload.GenderRatio is null ? variety.GenderRatio : GenderRatio.TryCreate(payload.GenderRatio.Value),
        payload.Genus is null ? variety.Genus : Genus.TryCreate(payload.Genus.Value),
        actorId);
    }

    await _varietyManager.EnsureUnicityAsync(variety, cancellationToken);
    await _varietyRepository.SaveAsync(variety, cancellationToken);

    return await _varietyQuerier.ReadAsync(variety, cancellationToken);
  }
}
