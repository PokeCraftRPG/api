using Logitar.CQRS;
using PokeGame.Core.Moves;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Species;
using PokeGame.Core.Storages;
using PokeGame.Core.Varieties.Models;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Varieties.Commands;

internal record CreateOrReplaceVarietyCommand(CreateOrReplaceVarietyPayload Payload, Guid? Id) : ICommand<CreateOrReplaceVarietyResult>;

internal class CreateOrReplaceVarietyCommandHandler : ICommandHandler<CreateOrReplaceVarietyCommand, CreateOrReplaceVarietyResult>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly ISpeciesManager _speciesManager;
  private readonly IStorageService _storageService;
  private readonly IVarietyManager _varietyManager;
  private readonly IVarietyQuerier _varietyQuerier;
  private readonly IVarietyRepository _varietyRepository;

  public CreateOrReplaceVarietyCommandHandler(
    IContext context,
    IPermissionService permissionService,
    ISpeciesManager speciesManager,
    IStorageService storageService,
    IVarietyManager varietyManager,
    IVarietyQuerier varietyQuerier,
    IVarietyRepository varietyRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _speciesManager = speciesManager;
    _storageService = storageService;
    _varietyManager = varietyManager;
    _varietyQuerier = varietyQuerier;
    _varietyRepository = varietyRepository;
  }

  public async Task<CreateOrReplaceVarietyResult> HandleAsync(CreateOrReplaceVarietyCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceVarietyPayload payload = command.Payload;
    payload.Validate();

    UserId userId = _context.UserId;
    WorldId worldId = _context.WorldId;

    VarietyId varietyId = VarietyId.NewId(worldId);
    Variety? variety = null;
    if (command.Id.HasValue)
    {
      varietyId = new(worldId, command.Id.Value);
      variety = await _varietyRepository.LoadAsync(varietyId, cancellationToken);
    }

    PokemonSpecies species = await _speciesManager.FindAsync(payload.Species, nameof(payload.Species), cancellationToken);
    Slug key = new(payload.Key);

    bool created = false;
    if (variety is null)
    {
      await _permissionService.CheckAsync(Actions.CreateVariety, cancellationToken);

      variety = new(species, payload.IsDefault, key, userId, varietyId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, variety, cancellationToken);

      if (species.Id != variety.SpeciesId)
      {
        throw new ImmutablePropertyException<Guid>(variety, variety.SpeciesId.EntityId, species.EntityId, nameof(payload.Species));
      }

      variety.SetDefault(payload.IsDefault, userId);
      variety.SetKey(key, userId);
    }

    variety.Name = Name.TryCreate(payload.Name);
    variety.Genus = Genus.TryCreate(payload.Genus);
    variety.Description = Description.TryCreate(payload.Description);

    variety.GenderRatio = payload.GenderRatio.HasValue ? new GenderRatio(payload.GenderRatio.Value) : null;

    variety.CanChangeForm = payload.CanChangeForm;

    variety.Url = Url.TryCreate(payload.Url);
    variety.Notes = Notes.TryCreate(payload.Notes);

    variety.Update(userId);

    IReadOnlyDictionary<MoveId, int?> moves = await _varietyManager.FindMovesAsync(payload.Moves, nameof(payload.Moves), cancellationToken);
    foreach (KeyValuePair<MoveId, Level?> move in variety.AllMoves)
    {
      if (!moves.ContainsKey(move.Key))
      {
        variety.RemoveMove(move.Key, userId);
      }
    }
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

    VarietyModel model = await _varietyQuerier.ReadAsync(variety, cancellationToken);
    return new CreateOrReplaceVarietyResult(model, created);
  }
}
