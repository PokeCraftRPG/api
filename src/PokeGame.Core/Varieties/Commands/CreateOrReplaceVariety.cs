using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Permissions;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Varieties.Commands;

internal record CreateOrReplaceVarietyCommand(CreateOrReplaceVarietyPayload Payload, Guid? Id) : ICommand<CreateOrReplaceVarietyResult>;

internal class CreateOrReplaceVarietyCommandHandler : ICommandHandler<CreateOrReplaceVarietyCommand, CreateOrReplaceVarietyResult>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly IVarietyManager _varietyManager;
  private readonly IVarietyQuerier _varietyQuerier;
  private readonly IVarietyRepository _varietyRepository;

  public CreateOrReplaceVarietyCommandHandler(
    IContext context,
    IPermissionService permissionService,
    ISpeciesRepository speciesRepository,
    IVarietyManager varietyManager,
    IVarietyQuerier varietyQuerier,
    IVarietyRepository varietyRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _speciesRepository = speciesRepository;
    _varietyManager = varietyManager;
    _varietyQuerier = varietyQuerier;
    _varietyRepository = varietyRepository;
  }

  public async Task<CreateOrReplaceVarietyResult> HandleAsync(CreateOrReplaceVarietyCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceVarietyPayload payload = command.Payload;
    payload.Validate();

    VarietyId varietyId = VarietyId.NewId(_context.WorldId);
    Variety? variety = null;
    if (command.Id.HasValue)
    {
      varietyId = new VarietyId(varietyId.WorldId, command.Id.Value);
      variety = await _varietyRepository.LoadAsync(varietyId, cancellationToken);
    }

    ActorId? actorId = _context.ActorId;
    Key key = new(payload.Key);

    bool created = false;
    if (variety is null)
    {
      await _permissionService.CheckAsync(Actions.CreateVariety, cancellationToken);

      SpeciesId speciesId = new(varietyId.WorldId, payload.SpeciesId);
      PokemonSpecies species = await _speciesRepository.LoadAsync(speciesId, cancellationToken)
        ?? throw new EntityNotFoundException(speciesId, nameof(payload.SpeciesId));

      variety = new Variety(varietyId, species.Id, key, actorId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, variety, cancellationToken);

      if (payload.SpeciesId != variety.SpeciesId.EntityId)
      {
        throw new ImmutablePropertyException<Guid>(variety, variety.SpeciesId.EntityId, payload.SpeciesId, nameof(payload.SpeciesId));
      }

      variety.SetKey(key, actorId);
    }

    variety.SetDefault(payload.IsDefault, actorId);

    variety.Update(
      Name.TryCreate(payload.Name),
      Summary.TryCreate(payload.Summary),
      Content.TryCreate(payload.Content),
      payload.CanChangeForm,
      GenderRatio.TryCreate(payload.GenderRatio),
      Genus.TryCreate(payload.Genus),
      actorId);

    await _varietyManager.EnsureUnicityAsync(variety, cancellationToken);
    await _varietyRepository.SaveAsync(variety, cancellationToken);

    VarietyDto dto = await _varietyQuerier.ReadAsync(variety, cancellationToken);
    return new CreateOrReplaceVarietyResult(dto, created);
  }
}
