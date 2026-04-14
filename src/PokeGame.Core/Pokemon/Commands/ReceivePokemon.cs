using Logitar.CQRS;
using PokeGame.Core.Items;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Regions;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Pokemon.Commands;

internal record ReceivePokemonCommand(Guid Id, ReceivePokemonPayload Payload) : ICommand<PokemonModel?>;

internal class ReceivePokemonCommandHandler : ICommandHandler<ReceivePokemonCommand, PokemonModel?>
{
  private readonly IContext _context;
  private readonly IItemManager _itemManager;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonQuerier _pokemonQuerier;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly ITrainerManager _trainerManager;

  public ReceivePokemonCommandHandler(
    IContext context,
    IItemManager itemManager,
    IPermissionService permissionService,
    IPokemonQuerier pokemonQuerier,
    IPokemonRepository pokemonRepository,
    ITrainerManager trainerManager)
  {
    _context = context;
    _itemManager = itemManager;
    _permissionService = permissionService;
    _pokemonQuerier = pokemonQuerier;
    _pokemonRepository = pokemonRepository;
    _trainerManager = trainerManager;
  }

  public async Task<PokemonModel?> HandleAsync(ReceivePokemonCommand command, CancellationToken cancellationToken)
  {
    ReceivePokemonPayload payload = command.Payload;
    payload.Validate();

    PokemonId pokemonId = new(_context.WorldId, command.Id);
    Specimen? specimen = await _pokemonRepository.LoadAsync(pokemonId, cancellationToken);
    if (specimen is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Receive, specimen, cancellationToken);

    Trainer trainer = await _trainerManager.FindAsync(payload.Trainer, nameof(payload.Trainer), cancellationToken);
    Item pokeBall = await _itemManager.FindAsync(payload.PokeBall, nameof(payload.PokeBall), cancellationToken);

    Location location = new(payload.Location);
    specimen.Receive(trainer, pokeBall, location, _context.UserId);

    // TODO(fpion): update storage

    await _pokemonRepository.SaveAsync(specimen, cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
