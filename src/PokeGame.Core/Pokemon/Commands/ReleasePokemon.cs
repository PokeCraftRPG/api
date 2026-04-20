using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Rosters;

namespace PokeGame.Core.Pokemon.Commands;

internal record ReleasePokemonCommand(Guid Id) : ICommand<PokemonModel?>;

internal class ReleasePokemonCommandHandler : ICommandHandler<ReleasePokemonCommand, PokemonModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonQuerier _pokemonQuerier;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly IRosterRepository _rosterRepository;

  public ReleasePokemonCommandHandler(
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

  public async Task<PokemonModel?> HandleAsync(ReleasePokemonCommand command, CancellationToken cancellationToken)
  {
    PokemonId pokemonId = new(_context.WorldId, command.Id);
    Specimen? specimen = await _pokemonRepository.LoadAsync(pokemonId, cancellationToken);
    if (specimen is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Release, specimen, cancellationToken);

    if (specimen.Ownership is null)
    {
      throw new PokemonHasNoOwnerException(specimen);
    }

    RosterId rosterId = new(specimen.Ownership.TrainerId);
    Roster roster = await _rosterRepository.LoadAsync(rosterId, cancellationToken) ?? new(rosterId);

    if (specimen.Slot is null || specimen.Slot.Box.HasValue)
    {
      PokemonParty party = new(specimen.Ownership.TrainerId);
      roster.Release(specimen, party, _context.UserId);

      await _pokemonRepository.SaveAsync(specimen, cancellationToken);
    }
    else
    {
      IEnumerable<PokemonId> partyIds = roster.GetParty().Except([specimen.Id]);
      IEnumerable<Specimen> members = (await _pokemonRepository.LoadAsync(partyIds, cancellationToken)).Concat([specimen]);

      PokemonParty party = new(members);
      roster.Release(specimen, party, _context.UserId);

      await _pokemonRepository.SaveAsync(party.Members, cancellationToken);
    }

    await _rosterRepository.SaveAsync(roster, cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
