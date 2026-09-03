using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species.Commands;

internal record SetRegionalNumberCommand(Guid SpeciesId, Guid RegionId, SetRegionalNumberPayload Payload) : ICommand<SpeciesDto>;

internal class SetRegionalNumberCommandHandler : ICommandHandler<SetRegionalNumberCommand, SpeciesDto>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IRegionRepository _regionRepository;
  private readonly ISpeciesManager _speciesManager;
  private readonly ISpeciesQuerier _speciesQuerier;
  private readonly ISpeciesRepository _speciesRepository;

  public SetRegionalNumberCommandHandler(
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

  public async Task<SpeciesDto> HandleAsync(SetRegionalNumberCommand command, CancellationToken cancellationToken)
  {
    SetRegionalNumberPayload payload = command.Payload;
    payload.Validate();

    SpeciesId speciesId = new(_context.WorldId, command.SpeciesId);
    PokemonSpecies species = await _speciesRepository.LoadAsync(speciesId, cancellationToken)
      ?? throw new EntityNotFoundException(speciesId, nameof(command.SpeciesId));
    await _permissionService.CheckAsync(Actions.Update, species, cancellationToken);

    RegionId regionId = new(species.WorldId, command.RegionId);
    Region region = await _regionRepository.LoadAsync(regionId, cancellationToken)
      ?? throw new EntityNotFoundException(regionId, nameof(command.RegionId));

    bool created = species.HasRegionalNumber(region);

    Number number = new(payload.Number);
    species.SetRegionalNumber(region, number, _context.ActorId);

    await _speciesManager.EnsureUnicityAsync(species, cancellationToken);
    await _speciesRepository.SaveAsync(species, cancellationToken);

    return await _speciesQuerier.ReadAsync(species, cancellationToken);
  }
}
