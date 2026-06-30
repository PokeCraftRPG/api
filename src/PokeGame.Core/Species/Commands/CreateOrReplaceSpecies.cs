using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions;
using PokeGame.Core.Species.Events;
using PokeGame.Core.Species.Models;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Species.Commands;

internal record CreateOrReplaceSpeciesCommand(CreateOrReplaceSpeciesPayload Payload, Guid? Id) : ICommand<CreateOrReplaceSpeciesResult>;

internal class CreateOrReplaceSpeciesCommandHandler : ICommandHandler<CreateOrReplaceSpeciesCommand, CreateOrReplaceSpeciesResult>
{

  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IRegionRepository _regionRepository;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly IWorldRepository _worldRepository;

  public CreateOrReplaceSpeciesCommandHandler(
    IContext context,
    IPermissionService permissionService,
    IRegionRepository regionRepository,
    ISpeciesRepository speciesRepository,
    IWorldRepository worldRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _regionRepository = regionRepository;
    _speciesRepository = speciesRepository;
    _worldRepository = worldRepository;
  }

  public async Task<CreateOrReplaceSpeciesResult> HandleAsync(CreateOrReplaceSpeciesCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceSpeciesPayload payload = command.Payload;
    payload.Validate();

    PokemonSpecies? species = null;
    if (command.Id.HasValue)
    {
      species = await _speciesRepository.LoadAsync(command.Id.Value, cancellationToken);
    }

    IReadOnlyDictionary<Region, int> regionalNumbers = await RegionalNumberResolver.ResolveAsync(
      _regionRepository,
      payload.RegionalNumbers,
      nameof(payload.RegionalNumbers),
      cancellationToken);

    bool created = false;
    if (species is null)
    {
      World world = await _worldRepository.LoadAsync(_context.WorldId, cancellationToken)
        ?? throw new InvalidOperationException($"The world 'Id={_context.WorldId}' was not loaded.");
      await _permissionService.CheckAsync(Actions.CreateSpecies, world, cancellationToken);

      species = new PokemonSpecies(
        world,
        payload.Number,
        payload.Key,
        payload.CatchRate,
        payload.EggCycles,
        _context.UserId,
        command.Id,
        payload.Category,
        payload.Name,
        payload.Description,
        payload.BaseFriendship,
        payload.GrowthRate,
        payload.EggGroups.Primary,
        payload.EggGroups.Secondary,
        regionalNumbers);
      _speciesRepository.Add(species);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, species, cancellationToken);

      if (payload.Number != species.Number)
      {
        throw new ImmutablePropertyException<int>(species, species.Number, payload.Number, nameof(PokemonSpecies.Number));
      }
      if (payload.Category != species.Category)
      {
        throw new ImmutablePropertyException<PokemonCategory>(species, species.Category, payload.Category, nameof(PokemonSpecies.Category));
      }

      SpeciesUpdated record = species.Update(
        payload.Key,
        payload.Name,
        payload.Description,
        payload.BaseFriendship,
        payload.CatchRate,
        payload.GrowthRate,
        payload.EggCycles,
        payload.EggGroups.Primary,
        payload.EggGroups.Secondary,
        regionalNumbers,
        _context.UserId);
      _speciesRepository.Update(species, record);
    }

    await _speciesRepository.EnsureUnicityAsync(species, cancellationToken);

    await _context.SaveChangesAsync(cancellationToken);

    SpeciesModel model = await _speciesRepository.ReadAsync(species, cancellationToken);
    return new CreateOrReplaceSpeciesResult(model, created);
  }
}
