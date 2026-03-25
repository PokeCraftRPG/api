using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions;
using PokeGame.Core.Species.Models;
using PokeGame.Core.Storages;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Species.Commands;

internal record CreateOrReplaceSpeciesCommand(CreateOrReplaceSpeciesPayload Payload, Guid? Id) : ICommand<CreateOrReplaceSpeciesResult>;

internal class CreateOrReplaceSpeciesCommandHandler : ICommandHandler<CreateOrReplaceSpeciesCommand, CreateOrReplaceSpeciesResult>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly ISpeciesManager _speciesManager;
  private readonly ISpeciesQuerier _speciesQuerier;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly IStorageService _storageService;

  public CreateOrReplaceSpeciesCommandHandler(
    IContext context,
    IPermissionService permissionService,
    ISpeciesManager speciesManager,
    ISpeciesQuerier speciesQuerier,
    ISpeciesRepository speciesRepository,
    IStorageService storageService)
  {
    _context = context;
    _permissionService = permissionService;
    _speciesManager = speciesManager;
    _speciesQuerier = speciesQuerier;
    _speciesRepository = speciesRepository;
    _storageService = storageService;
  }

  public async Task<CreateOrReplaceSpeciesResult> HandleAsync(CreateOrReplaceSpeciesCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceSpeciesPayload payload = command.Payload;
    payload.Validate();

    UserId userId = _context.UserId;
    WorldId worldId = _context.WorldId;

    SpeciesId speciesId = SpeciesId.NewId(worldId);
    SpeciesAggregate? species = null;
    if (command.Id.HasValue)
    {
      speciesId = new(worldId, command.Id.Value);
      species = await _speciesRepository.LoadAsync(speciesId, cancellationToken);
    }

    Slug key = new(payload.Key);
    Friendship baseFriendship = new(payload.BaseFriendship);
    CatchRate catchRate = new(payload.CatchRate);
    EggCycles eggCycles = new(payload.EggCycles);
    EggGroups eggGroups = new(payload.EggGroups);

    bool created = false;
    if (species is null)
    {
      await _permissionService.CheckAsync(Actions.CreateSpecies, cancellationToken);

      species = new(new Number(payload.Number), payload.Category, key, baseFriendship, catchRate, payload.GrowthRate, eggCycles, eggGroups, userId, speciesId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, species, cancellationToken);

      if (payload.Number != species.Number.Value)
      {
        throw new ImmutablePropertyException<int>(species, species.Number.Value, payload.Number, nameof(payload.Number));
      }
      if (payload.Category != species.Category)
      {
        throw new ImmutablePropertyException<PokemonCategory>(species, species.Category, payload.Category, nameof(payload.Category));
      }

      species.SetKey(key, userId);

      species.BaseFriendship = baseFriendship;
      species.CatchRate = catchRate;
      species.GrowthRate = payload.GrowthRate;

      species.EggCycles = eggCycles;
      species.EggGroups = eggGroups;
    }

    species.Name = Name.TryCreate(payload.Name);

    species.Url = Url.TryCreate(payload.Url);
    species.Notes = Notes.TryCreate(payload.Notes);

    species.Update(userId);

    IReadOnlyDictionary<RegionId, Number?> regionalNumbers = await _speciesManager.FindRegionalNumbersAsync(payload.RegionalNumbers, nameof(payload.RegionalNumbers), cancellationToken);
    foreach (KeyValuePair<RegionId, Number> regionalNumber in species.RegionalNumbers)
    {
      if (!regionalNumbers.ContainsKey(regionalNumber.Key))
      {
        species.SetRegionalNumber(regionalNumber.Key, number: null, userId);
      }
    }
    foreach (KeyValuePair<RegionId, Number?> regionalNumber in regionalNumbers)
    {
      species.SetRegionalNumber(regionalNumber.Key, regionalNumber.Value, userId);
    }

    await _speciesQuerier.EnsureUnicityAsync(species, cancellationToken);

    await _storageService.ExecuteWithQuotaAsync(
      species,
      async () => await _speciesRepository.SaveAsync(species, cancellationToken),
      cancellationToken);

    SpeciesModel model = await _speciesQuerier.ReadAsync(species, cancellationToken);
    return new CreateOrReplaceSpeciesResult(model, created);
  }
}
