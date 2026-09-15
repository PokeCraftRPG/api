using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Rosters.Models;

namespace PokeGame.Core.Rosters.Commands;

internal record SetRosterEntryCommand(Guid PokemonId, SetRosterEntryPayload Payload) : ICommand<PokemonDto?>;

internal class SetRosterEntryCommandHandler : ICommandHandler<SetRosterEntryCommand, PokemonDto?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonQuerier _pokemonQuerier;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly IRosterRepository _rosterRepository;

  public SetRosterEntryCommandHandler(
    IContext context,
    IPermissionService permissionService,
    IPokemonQuerier pokemonQuerier,
    IPokemonRepository pokemonRepository,
    IRosterRepository rosterRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _pokemonQuerier = pokemonQuerier;
    _pokemonRepository = pokemonRepository;
    _rosterRepository = rosterRepository;
  }

  public async Task<PokemonDto?> HandleAsync(SetRosterEntryCommand command, CancellationToken cancellationToken = default)
  {
    SetRosterEntryPayload payload = command.Payload;
    payload.Validate();

    PokemonId pokemonId = new(_context.WorldId, command.PokemonId);
    Specimen? specimen = await _pokemonRepository.LoadAsync(pokemonId, cancellationToken);
    if (specimen is null)
    {
      return null;
    }

    PokemonOwnership ownership = specimen.Ownership ?? throw new PokemonHasNoOwnerException(specimen);
    RosterId rosterId = new(ownership.TrainerId);
    Roster roster = await _rosterRepository.LoadAsync(rosterId, cancellationToken)
      ?? throw new InvalidOperationException($"The trainer 'Id={rosterId.TrainerId}' roster was not loaded.");
    await _permissionService.CheckAsync(Actions.ManageEntries, roster, cancellationToken);

    roster.SetEntry(specimen, payload.Priority, payload.TagIds, _context.ActorId);

    await _rosterRepository.SaveAsync(roster, cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
