using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Permissions;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species.Commands;

internal record CreateOrReplaceSpeciesCommand(CreateOrReplaceSpeciesPayload Payload, Guid? Id) : ICommand<CreateOrReplaceSpeciesResult>;

internal class CreateOrReplaceSpeciesCommandHandler : ICommandHandler<CreateOrReplaceSpeciesCommand, CreateOrReplaceSpeciesResult>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly ISpeciesManager _speciesManager;
  private readonly ISpeciesQuerier _speciesQuerier;
  private readonly ISpeciesRepository _speciesRepository;

  public CreateOrReplaceSpeciesCommandHandler(
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

  public async Task<CreateOrReplaceSpeciesResult> HandleAsync(CreateOrReplaceSpeciesCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceSpeciesPayload payload = command.Payload;
    payload.Validate();

    SpeciesId? speciesId = null;
    PokemonSpecies? species = null;
    if (command.Id.HasValue)
    {
      speciesId = new(_context.WorldId, command.Id.Value);
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

    // TODO(fpion): regional numbers

    await _speciesManager.EnsureUnicityAsync(species, cancellationToken);
    await _speciesRepository.SaveAsync(species, cancellationToken);

    SpeciesDto dto = await _speciesQuerier.ReadAsync(species, cancellationToken);
    return new CreateOrReplaceSpeciesResult(dto, created);
  }
}
