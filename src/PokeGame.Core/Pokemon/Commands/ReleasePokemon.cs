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

    UserId userId = _context.UserId;

    Roster? roster = null;
    if (specimen.Ownership is not null)
    {
      RosterId rosterId = new(specimen.Ownership.TrainerId);
      roster = await _rosterRepository.LoadAsync(rosterId, cancellationToken);
      roster?.Remove(specimen, userId);
    }

    specimen.Release(userId);

    await _pokemonRepository.SaveAsync(specimen, cancellationToken);

    if (roster is not null)
    {
      await _rosterRepository.SaveAsync(roster, cancellationToken);
    }

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
