using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species.Commands;

internal record RemoveRegionalNumberCommand(Guid SpeciesId, Guid RegionId) : ICommand<SpeciesDto?>;

internal class RemoveRegionalNumberCommandHandler : ICommandHandler<RemoveRegionalNumberCommand, SpeciesDto?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly ISpeciesQuerier _speciesQuerier;
  private readonly ISpeciesRepository _speciesRepository;

  public RemoveRegionalNumberCommandHandler(
    IContext context,
    IPermissionService permissionService,
    ISpeciesQuerier speciesQuerier,
    ISpeciesRepository speciesRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _speciesQuerier = speciesQuerier;
    _speciesRepository = speciesRepository;
  }

  public async Task<SpeciesDto?> HandleAsync(RemoveRegionalNumberCommand command, CancellationToken cancellationToken)
  {
    SpeciesId speciesId = new(_context.WorldId, command.SpeciesId);
    PokemonSpecies? species = await _speciesRepository.LoadAsync(speciesId, cancellationToken);
    if (species is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, species, cancellationToken);

    RegionId regionId = new(species.WorldId, command.RegionId);
    species.RemoveRegionalNumber(regionId, _context.ActorId);

    await _speciesRepository.SaveAsync(species, cancellationToken);

    return await _speciesQuerier.ReadAsync(species, cancellationToken);
  }
}
