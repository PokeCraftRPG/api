using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions;
using PokeGame.Core.Species.Models;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Species.Commands;

internal record UpdateSpeciesCommand(Guid Id, UpdateSpeciesPayload Payload) : ICommand<SpeciesModel?>;

internal class UpdateSpeciesCommandHandler : ICommandHandler<UpdateSpeciesCommand, SpeciesModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly ISpeciesManager _speciesManager;
  private readonly ISpeciesQuerier _speciesQuerier;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly IStorageService _storageService;

  public UpdateSpeciesCommandHandler(
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

    UserId userId = _context.UserId;

    if (!string.IsNullOrWhiteSpace(payload.Key))
    {
      Slug key = new(payload.Key);
      species.SetKey(key, userId);
    }
    if (payload.Name is not null)
    {
      species.Name = Name.TryCreate(payload.Name.Value);
    }

    if (payload.BaseFriendship.HasValue)
    {
      species.BaseFriendship = new Friendship(payload.BaseFriendship.Value);
    }
    if (payload.CatchRate.HasValue)
    {
      species.CatchRate = new CatchRate(payload.CatchRate.Value);
    }
    if (payload.GrowthRate.HasValue)
    {
      species.GrowthRate = payload.GrowthRate.Value;
    }

    if (payload.EggCycles.HasValue)
    {
      species.EggCycles = new EggCycles(payload.EggCycles.Value);
    }
    if (payload.EggGroups is not null)
    {
      species.EggGroups = new EggGroups(payload.EggGroups);
    }

    if (payload.Url is not null)
    {
      species.Url = Url.TryCreate(payload.Url.Value);
    }
    if (payload.Notes is not null)
    {
      species.Notes = Notes.TryCreate(payload.Notes.Value);
    }

    species.Update(userId);

    IReadOnlyDictionary<RegionId, Number?> regionalNumbers = await _speciesManager.FindRegionalNumbersAsync(payload.RegionalNumbers, nameof(payload.RegionalNumbers), cancellationToken);
    foreach (KeyValuePair<RegionId, Number?> regionalNumber in regionalNumbers)
    {
      species.SetRegionalNumber(regionalNumber.Key, regionalNumber.Value, userId);
    }

    await _speciesQuerier.EnsureUnicityAsync(species, cancellationToken);

    await _storageService.ExecuteWithQuotaAsync(
      species,
      async () => await _speciesRepository.SaveAsync(species, cancellationToken),
      cancellationToken);

    return await _speciesQuerier.ReadAsync(species, cancellationToken);
  }
}
