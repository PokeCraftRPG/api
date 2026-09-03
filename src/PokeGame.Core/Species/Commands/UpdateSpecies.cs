using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Permissions;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species.Commands;

internal record UpdateSpeciesCommand(Guid Id, UpdateSpeciesPayload Payload) : ICommand<SpeciesDto?>;

internal class UpdateSpeciesCommandHandler : ICommandHandler<UpdateSpeciesCommand, SpeciesDto?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly ISpeciesManager _speciesManager;
  private readonly ISpeciesQuerier _speciesQuerier;
  private readonly ISpeciesRepository _speciesRepository;

  public UpdateSpeciesCommandHandler(
    IContext context,
    IPermissionService permissionService,
    ISpeciesManager speciesManager,
    ISpeciesQuerier speciesQuerier,
    ISpeciesRepository speciesRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _speciesManager = speciesManager;
    _speciesQuerier = speciesQuerier;
    _speciesRepository = speciesRepository;
  }

  public async Task<SpeciesDto?> HandleAsync(UpdateSpeciesCommand command, CancellationToken cancellationToken)
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
      species.SetKey(new Key(payload.Key), actorId);
    }

    if (payload.Name is not null || payload.Summary is not null || payload.Content is not null
      || payload.BaseFriendship is not null || payload.CatchRate is not null || payload.GrowthRate is not null
      || payload.Eggs is not null)
    {
      species.Update(
        payload.Name is null ? species.Name : Name.TryCreate(payload.Name.Value),
        payload.Summary is null ? species.Summary : Summary.TryCreate(payload.Summary.Value),
        payload.Content is null ? species.Content : Content.TryCreate(payload.Content.Value),
        payload.BaseFriendship is null ? species.BaseFriendship : new Friendship(payload.BaseFriendship.Value),
        payload.CatchRate is null ? species.CatchRate : new CatchRate(payload.CatchRate.Value),
        payload.GrowthRate is null ? species.GrowthRate : payload.GrowthRate.Value,
        payload.Eggs is null ? species.Eggs : SpeciesEggs.From(payload.Eggs),
        actorId);
    }

    await _speciesManager.EnsureUnicityAsync(species, cancellationToken);
    await _speciesRepository.SaveAsync(species, cancellationToken);

    return await _speciesQuerier.ReadAsync(species, cancellationToken);
  }
}
