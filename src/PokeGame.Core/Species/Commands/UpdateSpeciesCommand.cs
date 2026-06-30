using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions;
using PokeGame.Core.Species.Events;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species.Commands;

internal record UpdateSpeciesCommand(Guid Id, UpdateSpeciesPayload Payload) : ICommand<SpeciesModel?>;

internal class UpdateSpeciesCommandHandler : ICommandHandler<UpdateSpeciesCommand, SpeciesModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IRegionRepository _regionRepository;
  private readonly ISpeciesRepository _speciesRepository;

  public UpdateSpeciesCommandHandler(
    IContext context,
    IPermissionService permissionService,
    IRegionRepository regionRepository,
    ISpeciesRepository speciesRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _regionRepository = regionRepository;
    _speciesRepository = speciesRepository;
  }

  public async Task<SpeciesModel?> HandleAsync(UpdateSpeciesCommand command, CancellationToken cancellationToken)
  {
    UpdateSpeciesPayload payload = command.Payload;
    payload.Validate();

    PokemonSpecies? species = await _speciesRepository.LoadAsync(command.Id, cancellationToken);
    if (species is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, species, cancellationToken);

    IReadOnlyDictionary<Region, int>? regionalNumbers = null;
    if (payload.RegionalNumbers.Count > 0)
    {
      regionalNumbers = await RegionalNumberResolver.ResolveAsync(
        _regionRepository,
        payload.RegionalNumbers,
        nameof(payload.RegionalNumbers),
        cancellationToken);
    }

    SpeciesUpdated record = species.Update(
      string.IsNullOrWhiteSpace(payload.Key) ? species.Key : payload.Key,
      payload.Name is null ? species.Name : payload.Name.Value,
      payload.Description is null ? species.Description : payload.Description.Value,
      payload.BaseFriendship ?? species.BaseFriendship,
      payload.CatchRate ?? species.CatchRate,
      payload.GrowthRate ?? species.GrowthRate,
      payload.EggCycles ?? species.EggCycles,
      payload.EggGroups?.Primary ?? species.PrimaryEggGroup,
      payload.EggGroups?.Secondary ?? species.SecondaryEggGroup,
      regionalNumbers,
      _context.UserId);
    _speciesRepository.Update(species, record);

    await _speciesRepository.EnsureUnicityAsync(species, cancellationToken);

    await _context.SaveChangesAsync(cancellationToken);

    return await _speciesRepository.ReadAsync(species, cancellationToken);
  }
}
