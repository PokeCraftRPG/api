using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Rosters;

namespace PokeGame.Core.Pokemon.Commands;

internal record DepositPokemonCommand(Guid Id) : ICommand<PokemonModel?>;

internal class DepositPokemonCommandHandler : ICommandHandler<DepositPokemonCommand, PokemonModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonQuerier _pokemonQuerier;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly IRosterRepository _rosterRepository;

  public DepositPokemonCommandHandler(
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

  public async Task<PokemonModel?> HandleAsync(DepositPokemonCommand command, CancellationToken cancellationToken)
  {
    PokemonId pokemonId = new(_context.WorldId, command.Id);
    Specimen? specimen = await _pokemonRepository.LoadAsync(pokemonId, cancellationToken);
    if (specimen is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Deposit, specimen, cancellationToken);

    if (specimen.Ownership is null || specimen.Slot is null)
    {
      throw new PokemonHasNoOwnerException(specimen);
    }

    RosterId rosterId = new(specimen.Ownership.TrainerId);
    Roster roster = await _rosterRepository.LoadAsync(rosterId, cancellationToken) ?? new(rosterId);

    IEnumerable<PokemonId> memberIds = roster.GetParty().Except([specimen.Id]);
    List<Specimen> members = new(await _pokemonRepository.LoadAsync(memberIds, cancellationToken));
    if (!specimen.Slot.Box.HasValue)
    {
      members.Add(specimen);
    }

    PokemonParty party = new(members);
    roster.Deposit(specimen, party, _context.UserId);

    await _pokemonRepository.SaveAsync(party.Members, cancellationToken);
    await _rosterRepository.SaveAsync(roster, cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
