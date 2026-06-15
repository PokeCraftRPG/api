using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species.Commands;

internal record CreateOrReplaceSpeciesCommand(CreateOrReplaceSpeciesPayload Payload, Guid? Id) : ICommand<CreateOrReplaceSpeciesResult>;

internal class CreateOrReplaceSpeciesCommandHandler : ICommandHandler<CreateOrReplaceSpeciesCommand, CreateOrReplaceSpeciesResult>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IRegionManager _regionManager;
  private readonly ISpeciesManager _speciesManager;
  private readonly ISpeciesQuerier _speciesQuerier;
  private readonly ISpeciesRepository _speciesRepository;

  public CreateOrReplaceSpeciesCommandHandler(
    IContext context,
    IPermissionService permissionService,
    IRegionManager regionManager,
    ISpeciesManager speciesManager,
    ISpeciesQuerier speciesQuerier,
    ISpeciesRepository speciesRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _regionManager = regionManager;
    _speciesManager = speciesManager;
    _speciesQuerier = speciesQuerier;
    _speciesRepository = speciesRepository;
  }

  public async Task<CreateOrReplaceSpeciesResult> HandleAsync(CreateOrReplaceSpeciesCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceSpeciesPayload payload = command.Payload;
    payload.Validate();

    SpeciesId speciesId = SpeciesId.NewId(_context.WorldId);
    PokemonSpecies? species = null;
    if (command.Id.HasValue)
    {
      speciesId = new SpeciesId(speciesId.WorldId, command.Id.Value);
      species = await _speciesRepository.LoadAsync(speciesId, cancellationToken);
    }

    Slug key = new(payload.Key);
    Friendship baseFriendship = new(payload.BaseFriendship);
    CatchRate catchRate = new(payload.CatchRate);
    EggCycles eggCycles = new(payload.EggCycles);
    EggGroups eggGroups = new(payload.EggGroups);
    ActorId? actorId = _context.ActorId;

    bool created = false;
    if (species is null)
    {
      await _permissionService.CheckAsync(Actions.CreateSpecies, cancellationToken);

      species = new PokemonSpecies(speciesId, new Number(payload.Number), payload.Category, key, catchRate, eggCycles, baseFriendship, payload.GrowthRate, eggGroups, actorId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, species, cancellationToken);

      if (payload.Number != species.Number.Value)
      {
        throw new NotImplementedException(); // TODO(fpion): implement
      }
      if (payload.Category != species.Category)
      {
        throw new NotImplementedException(); // TODO(fpion): implement
      }

      species.SetKey(key, actorId);
      species.SetGameData(baseFriendship, catchRate, payload.GrowthRate, eggCycles, eggGroups, actorId);
    }

    species.Rename(Name.TryCreate(payload.Name), actorId);
    species.Describe(Description.TryCreate(payload.Description), actorId);

    IReadOnlyDictionary<RegionId, Number?> regionalNumbers = await _regionManager.FindRegionalNumbersAsync(payload.RegionalNumbers, nameof(payload.RegionalNumbers), cancellationToken);
    foreach (RegionId regionId in species.RegionalNumbers.Keys)
    {
      if (!regionalNumbers.ContainsKey(regionId))
      {
        species.RemoveRegionalNumber(regionId, actorId);
      }
    }
    foreach (KeyValuePair<RegionId, Number?> regionalNumber in regionalNumbers)
    {
      if (regionalNumber.Value is null)
      {
        species.RemoveRegionalNumber(regionalNumber.Key, actorId);
      }
      else
      {
        species.SetRegionalNumber(regionalNumber.Key, regionalNumber.Value, actorId);
      }
    }

    await _speciesManager.EnsureUnicityAsync(species, cancellationToken);
    await _speciesRepository.SaveAsync(species, cancellationToken);

    SpeciesModel model = await _speciesQuerier.FindAsync(species, cancellationToken);
    return new CreateOrReplaceSpeciesResult(model, created);
  }
}
