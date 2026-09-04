using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions;
using PokeGame.Core.Species.Models;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Species.Commands;

internal record CreateOrReplaceSpeciesCommand(CreateOrReplaceSpeciesPayload Payload, Guid? Id) : ICommand<CreateOrReplaceSpeciesResult>;

internal class CreateOrReplaceSpeciesCommandHandler : ICommandHandler<CreateOrReplaceSpeciesCommand, CreateOrReplaceSpeciesResult>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IRegionRepository _regionRepository;
  private readonly ISpeciesManager _speciesManager;
  private readonly ISpeciesQuerier _speciesQuerier;
  private readonly ISpeciesRepository _speciesRepository;

  public CreateOrReplaceSpeciesCommandHandler(
    IContext context,
    IPermissionService permissionService,
    IRegionRepository regionRepository,
    ISpeciesManager speciesManager,
    ISpeciesQuerier speciesQuerier,
    ISpeciesRepository speciesRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _regionRepository = regionRepository;
    _speciesManager = speciesManager;
    _speciesQuerier = speciesQuerier;
    _speciesRepository = speciesRepository;
  }

  public async Task<CreateOrReplaceSpeciesResult> HandleAsync(CreateOrReplaceSpeciesCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceSpeciesPayload payload = command.Payload;
    payload.Validate();

    SpeciesId? speciesId = null;
    PokemonSpecies? species = null;
    if (command.Id.HasValue)
    {
      speciesId = new SpeciesId(_context.WorldId, command.Id.Value);
      species = await _speciesRepository.LoadAsync(speciesId.Value, cancellationToken);
    }

    ActorId? actorId = _context.ActorId;
    Key key = new(payload.Key);
    Number number = new(payload.Number);

    bool created = false;
    if (species is null)
    {
      await _permissionService.CheckAsync(Actions.CreateSpecies, cancellationToken);

      species = new PokemonSpecies(speciesId ?? SpeciesId.NewId(_context.WorldId), number, payload.Category, key, actorId);
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
        throw new ImmutablePropertyException<SpeciesCategory>(species, species.Category, payload.Category, nameof(payload.Category));
      }

      species.SetKey(key, actorId);
    }

    species.Update(
      Name.TryCreate(payload.Name),
      Summary.TryCreate(payload.Summary),
      Content.TryCreate(payload.Content),
      new Friendship(payload.BaseFriendship),
      new CatchRate(payload.CatchRate),
      payload.GrowthRate,
      SpeciesEggs.From(payload.Eggs),
      actorId);

    await SetRegionalNumbersAsync(payload, species, actorId, cancellationToken);

    await _speciesManager.EnsureUnicityAsync(species, cancellationToken);
    await _speciesRepository.SaveAsync(species, cancellationToken);

    SpeciesDto dto = await _speciesQuerier.ReadAsync(species, cancellationToken);
    return new CreateOrReplaceSpeciesResult(dto, created);
  }

  private async Task SetRegionalNumbersAsync(CreateOrReplaceSpeciesPayload payload, PokemonSpecies species, ActorId? actorId, CancellationToken cancellationToken)
  {
    WorldId worldId = species.WorldId;

    HashSet<Guid> entityIds = payload.RegionalNumbers.Select(x => x.RegionId).ToHashSet();
    HashSet<RegionId> regionIds = entityIds.Select(entityId => new RegionId(worldId, entityId)).ToHashSet();
    Dictionary<Guid, Region> regionsById = (await _regionRepository.LoadAsync(regionIds, cancellationToken)).ToDictionary(x => x.EntityId, x => x);

    IEnumerable<Guid> missingRegionIds = regionIds.Select(id => id.EntityId).Except(regionsById.Keys);
    if (missingRegionIds.Any())
    {
      throw new RegionsNotFoundException(worldId, missingRegionIds, nameof(payload.RegionalNumbers));
    }

    IEnumerable<RegionId> removedRegionIds = species.RegionalNumbers.Keys.Except(regionIds);
    foreach (RegionId regionId in removedRegionIds)
    {
      species.RemoveRegionalNumber(regionId, actorId);
    }

    foreach (RegionalNumberPayload regionalNumber in payload.RegionalNumbers)
    {
      Region region = regionsById[regionalNumber.RegionId];
      Number number = new(regionalNumber.Number);
      species.SetRegionalNumber(region, number, actorId);
    }
  }
}
