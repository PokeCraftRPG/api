using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Rosters;

namespace PokeGame.Core.Pokemon.Commands;

internal record WithdrawPokemonCommand(Guid Id) : ICommand<PokemonModel?>;

internal class WithdrawPokemonCommandHandler : ICommandHandler<WithdrawPokemonCommand, PokemonModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonQuerier _pokemonQuerier;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly IRosterRepository _rosterRepository;

  public WithdrawPokemonCommandHandler(
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

  public async Task<PokemonModel?> HandleAsync(WithdrawPokemonCommand command, CancellationToken cancellationToken)
  {
    PokemonId pokemonId = new(_context.WorldId, command.Id);
    Specimen? specimen = await _pokemonRepository.LoadAsync(pokemonId, cancellationToken);
    if (specimen is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Withdraw, specimen, cancellationToken);

    if (specimen.Ownership is null)
    {
      throw new NotImplementedException(); // TODO(fpion): implement
    }

    RosterId rosterId = new(specimen.Ownership.TrainerId);
    Roster roster = await _rosterRepository.LoadAsync(rosterId, cancellationToken) ?? new(rosterId);
    roster.Withdraw(specimen, _context.UserId);

    await _pokemonRepository.SaveAsync(specimen, cancellationToken);
    await _rosterRepository.SaveAsync(roster, cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
