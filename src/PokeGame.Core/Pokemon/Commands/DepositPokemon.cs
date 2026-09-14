using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Rosters;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon.Commands;

internal record DepositPokemonCommand(Guid Id) : ICommand<PokemonDto?>;

internal class DepositPokemonCommandHandler : ICommandHandler<DepositPokemonCommand, PokemonDto?>
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

  public async Task<PokemonDto?> HandleAsync(DepositPokemonCommand command, CancellationToken cancellationToken)
  {
    ActorId? actorId = _context.ActorId;
    WorldId worldId = _context.WorldId;

    PokemonId pokemonId = new(worldId, command.Id);
    Specimen? specimen = await _pokemonRepository.LoadAsync(pokemonId, cancellationToken);
    if (specimen is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Deposit, specimen, cancellationToken);

    PokemonOwnership ownership = specimen.Ownership ?? throw new PokemonHasNoOwnerException(specimen);
    RosterId rosterId = new(ownership.TrainerId);
    Roster roster = await _rosterRepository.LoadAsync(rosterId, cancellationToken)
      ?? throw new InvalidOperationException($"The trainer 'Id={rosterId.TrainerId}' roster was not loaded.");
    roster.Deposit(specimen, actorId);

    await _rosterRepository.SaveAsync(roster, cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
