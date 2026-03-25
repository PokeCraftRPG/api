using Logitar.CQRS;
using PokeGame.Core.Moves;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Storages;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Varieties.Commands;

internal record UpdateVarietyCommand(Guid Id, UpdateVarietyPayload Payload) : ICommand<VarietyModel?>;

internal class UpdateVarietyCommandHandler : ICommandHandler<UpdateVarietyCommand, VarietyModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IStorageService _storageService;
  private readonly IVarietyManager _varietyManager;
  private readonly IVarietyQuerier _varietyQuerier;
  private readonly IVarietyRepository _varietyRepository;

  public UpdateVarietyCommandHandler(
    IContext context,
    IPermissionService permissionService,
    IStorageService storageService,
    IVarietyManager varietyManager,
    IVarietyQuerier varietyQuerier,
    IVarietyRepository varietyRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _storageService = storageService;
    _varietyManager = varietyManager;
    _varietyQuerier = varietyQuerier;
    _varietyRepository = varietyRepository;
  }

  public async Task<VarietyModel?> HandleAsync(UpdateVarietyCommand command, CancellationToken cancellationToken)
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

    UserId userId = _context.UserId;

    if (payload.IsDefault.HasValue)
    {
      variety.SetDefault(payload.IsDefault.Value, userId);
    }

    if (!string.IsNullOrWhiteSpace(payload.Key))
    {
      Slug key = new(payload.Key);
      variety.SetKey(key, userId);
    }
    if (payload.Name is not null)
    {
      variety.Name = Name.TryCreate(payload.Name.Value);
    }
    if (payload.Genus is not null)
    {
      variety.Genus = Genus.TryCreate(payload.Genus.Value);
    }
    if (payload.Description is not null)
    {
      variety.Description = Description.TryCreate(payload.Description.Value);
    }

    if (payload.GenderRatio is not null)
    {
      variety.GenderRatio = payload.GenderRatio.Value.HasValue ? new GenderRatio(payload.GenderRatio.Value.Value) : null;
    }

    if (payload.CanChangeForm.HasValue)
    {
      variety.CanChangeForm = payload.CanChangeForm.Value;
    }

    if (payload.Url is not null)
    {
      variety.Url = Url.TryCreate(payload.Url.Value);
    }
    if (payload.Notes is not null)
    {
      variety.Notes = Notes.TryCreate(payload.Notes.Value);
    }

    variety.Update(userId);

    IReadOnlyDictionary<MoveId, int?> moves = await _varietyManager.FindMovesAsync(payload.Moves, nameof(payload.Moves), cancellationToken);
    foreach (KeyValuePair<MoveId, int?> move in moves)
    {
      if (!move.Value.HasValue)
      {
        variety.RemoveMove(move.Key, userId);
      }
      else if (move.Value.Value == 0)
      {
        variety.SetEvolutionMove(move.Key, userId);
      }
      else
      {
        variety.SetLevelMove(move.Key, new Level(move.Value.Value), userId);
      }
    }

    await _varietyQuerier.EnsureUnicityAsync(variety, cancellationToken);

    await _storageService.ExecuteWithQuotaAsync(
      variety,
      async () => await _varietyRepository.SaveAsync(variety, cancellationToken),
      cancellationToken);

    return await _varietyQuerier.ReadAsync(variety, cancellationToken);
  }
}
