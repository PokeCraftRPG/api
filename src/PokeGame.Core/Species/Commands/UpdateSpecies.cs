using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species.Commands;

internal record UpdateSpeciesCommand(Guid Id, UpdateSpeciesPayload Payload) : ICommand<SpeciesModel?>;

internal class UpdateSpeciesCommandHandler : ICommandHandler<UpdateSpeciesCommand, SpeciesModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IRegionManager _regionManager;
  private readonly ISpeciesManager _speciesManager;
  private readonly ISpeciesQuerier _speciesQuerier;
  private readonly ISpeciesRepository _speciesRepository;

  public UpdateSpeciesCommandHandler(
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

  public async Task<SpeciesModel?> HandleAsync(UpdateSpeciesCommand command, CancellationToken cancellationToken)
  {
    UpdateSpeciesPayload payload = command.Payload;
    payload.Validate();

    SpeciesId speciesId = new(_context.WorldId, command.Id);
    PokemonSpecies? species = await _speciesRepository.LoadAsync(speciesId, cancellationToken);
    if (species is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, species, cancellationToken);

    ActorId? actorId = _context.ActorId;

    if (!string.IsNullOrWhiteSpace(payload.Key))
    {
      species.SetKey(new Slug(payload.Key), actorId);
    }
    if (payload.Name is not null)
    {
      species.Rename(Name.TryCreate(payload.Name.Value), actorId);
    }
    if (payload.Description is not null)
    {
      species.Describe(Description.TryCreate(payload.Description.Value), actorId);
    }

    Friendship baseFriendship = Friendship.TryCreate(payload.BaseFriendship) ?? species.BaseFriendship;
    CatchRate catchRate = CatchRate.TryCreate(payload.CatchRate) ?? species.CatchRate;
    GrowthRate growthRate = payload.GrowthRate ?? species.GrowthRate;
    EggCycles eggCycles = EggCycles.TryCreate(payload.EggCycles) ?? species.EggCycles;
    EggGroups eggGroups = payload.EggGroups is null ? species.EggGroups : new(species.EggGroups);
    species.SetGameData(baseFriendship, catchRate, growthRate, eggCycles, eggGroups, actorId);

    IReadOnlyDictionary<RegionId, Number?> regionalNumbers = await _regionManager.FindRegionalNumbersAsync(payload.RegionalNumbers, nameof(payload.RegionalNumbers), cancellationToken);
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

    return await _speciesQuerier.FindAsync(species, cancellationToken);
  }
}
