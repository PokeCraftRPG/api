using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Rosters;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon.Commands;

internal record ReleasePokemonCommand(Guid Id) : ICommand<PokemonDto?>;

internal class ReleasePokemonCommandHandler : ICommandHandler<ReleasePokemonCommand, PokemonDto?>
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

  public async Task<PokemonDto?> HandleAsync(ReleasePokemonCommand command, CancellationToken cancellationToken)
  {
    ActorId? actorId = _context.ActorId;
    WorldId worldId = _context.WorldId;

    PokemonId pokemonId = new(worldId, command.Id);
    Specimen? specimen = await _pokemonRepository.LoadAsync(pokemonId, cancellationToken);
    if (specimen is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, specimen, cancellationToken);

    Roster? roster = null;
    if (specimen.Ownership is not null)
    {
      RosterId rosterId = new(specimen.Ownership.TrainerId);
      roster = await _rosterRepository.LoadAsync(rosterId, cancellationToken);
      if (roster is not null)
      {
        await _permissionService.CheckAsync(Actions.Update, roster, cancellationToken);
      }
    }

    specimen.Release(actorId);
    roster?.Remove(specimen, actorId);

    await _pokemonRepository.SaveAsync(specimen, cancellationToken);
    if (roster is not null)
    {
      await _rosterRepository.SaveAsync(roster, cancellationToken);
    }

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
