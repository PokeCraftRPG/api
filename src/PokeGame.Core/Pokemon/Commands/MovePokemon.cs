using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Rosters;

namespace PokeGame.Core.Pokemon.Commands;

internal record MovePokemonCommand(Guid Id, MovePokemonPayload Payload) : ICommand<PokemonModel?>;

internal class MovePokemonCommandHandler : ICommandHandler<MovePokemonCommand, PokemonModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonQuerier _pokemonQuerier;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly IRosterRepository _rosterRepository;

  public MovePokemonCommandHandler(
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

  public async Task<PokemonModel?> HandleAsync(MovePokemonCommand command, CancellationToken cancellationToken)
  {
    MovePokemonPayload payload = command.Payload;
    payload.Validate();

    PokemonId pokemonId = new(_context.WorldId, command.Id);
    Specimen? specimen = await _pokemonRepository.LoadAsync(pokemonId, cancellationToken);
    if (specimen is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Move, specimen, cancellationToken);

    if (specimen.Ownership is null || specimen.Slot is null)
    {
      throw new PokemonHasNoOwnerException(specimen);
    }

    Roster roster = await _rosterRepository.LoadAsync(specimen.Ownership.TrainerId, cancellationToken);
    PokemonSlot slot = new(payload.Position, payload.Box);

    if (specimen.Slot.Box.HasValue)
    {
      PokemonParty party = new(specimen.Ownership.TrainerId);
      roster.Move(specimen, slot, party, _context.UserId);

      await _pokemonRepository.SaveAsync(specimen, cancellationToken);
    }
    else
    {
      IEnumerable<PokemonId> partyIds = roster.GetParty().Except([specimen.Id]);
      IEnumerable<Specimen> members = (await _pokemonRepository.LoadAsync(partyIds, cancellationToken)).Concat([specimen]);

      PokemonParty party = new(members);
      roster.Move(specimen, slot, party, _context.UserId);

      await _pokemonRepository.SaveAsync(party.Members, cancellationToken);
    }

    await _rosterRepository.SaveAsync(roster, cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
