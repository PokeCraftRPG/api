using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Rosters;
using PokeGame.Core.Trainers;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon.Commands;

internal record WithdrawPokemonCommand(Guid Id) : ICommand<PokemonDto?>;

internal class WithdrawPokemonCommandHandler : ICommandHandler<WithdrawPokemonCommand, PokemonDto?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonQuerier _pokemonQuerier;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly IRosterRepository _rosterRepository;
  private readonly ITrainerRepository _trainerRepository;

  public WithdrawPokemonCommandHandler(
    IContext context,
    IPermissionService permissionService,
    IPokemonQuerier pokemonQuerier,
    IPokemonRepository pokemonRepository,
    IRosterRepository rosterRepository,
    ITrainerRepository trainerRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _pokemonQuerier = pokemonQuerier;
    _pokemonRepository = pokemonRepository;
    _rosterRepository = rosterRepository;
    _trainerRepository = trainerRepository;
  }

  public async Task<PokemonDto?> HandleAsync(WithdrawPokemonCommand command, CancellationToken cancellationToken)
  {
    ActorId? actorId = _context.ActorId;
    WorldId worldId = _context.WorldId;

    PokemonId pokemonId = new(worldId, command.Id);
    Specimen? specimen = await _pokemonRepository.LoadAsync(pokemonId, cancellationToken);
    if (specimen is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Withdraw, specimen, cancellationToken);

    PokemonOwnership ownership = specimen.Ownership ?? throw new PokemonHasNoOwnerException(specimen);
    Trainer trainer = await _trainerRepository.LoadAsync(ownership.TrainerId, cancellationToken)
      ?? throw new InvalidOperationException($"The trainer 'Id={ownership.TrainerId}' was not loaded.");

    RosterId rosterId = new(trainer.Id);
    Roster roster = await _rosterRepository.LoadAsync(rosterId, cancellationToken) ?? new(trainer); // TODO(fpion): this should be an error.
    roster.Withdraw(specimen, trainer, actorId);

    await _rosterRepository.SaveAsync(roster, cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
