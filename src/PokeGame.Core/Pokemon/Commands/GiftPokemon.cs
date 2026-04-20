using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Regions;
using PokeGame.Core.Rosters;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Pokemon.Commands;

internal record GiftPokemonCommand(Guid Id, GiftPokemonPayload Payload) : ICommand<PokemonModel?>;

internal class GiftPokemonCommandHandler : ICommandHandler<GiftPokemonCommand, PokemonModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonQuerier _pokemonQuerier;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly IRosterRepository _rosterRepository;
  private readonly ITrainerManager _trainerManager;

  public GiftPokemonCommandHandler(
    IContext context,
    IPermissionService permissionService,
    IPokemonQuerier pokemonQuerier,
    IPokemonRepository pokemonRepository,
    IRosterRepository rosterRepository,
    ITrainerManager trainerManager)
  {
    _context = context;
    _permissionService = permissionService;
    _pokemonQuerier = pokemonQuerier;
    _pokemonRepository = pokemonRepository;
    _rosterRepository = rosterRepository;
    _trainerManager = trainerManager;
  }

  public async Task<PokemonModel?> HandleAsync(GiftPokemonCommand command, CancellationToken cancellationToken)
  {
    GiftPokemonPayload payload = command.Payload;
    payload.Validate();

    PokemonId pokemonId = new(_context.WorldId, command.Id);
    Specimen? specimen = await _pokemonRepository.LoadAsync(pokemonId, cancellationToken);
    if (specimen is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Gift, specimen, cancellationToken);

    if (specimen.Ownership is null || specimen.Slot is null)
    {
      throw new PokemonHasNoOwnerException(specimen);
    }

    Trainer trainer = await _trainerManager.FindAsync(payload.Trainer, nameof(payload.Trainer), cancellationToken);
    if (specimen.Ownership.TrainerId != trainer.Id)
    {
      UserId userId = _context.UserId;

      Roster sourceRoster = await _rosterRepository.LoadAsync(specimen.Ownership.TrainerId, cancellationToken);
      PokemonParty party = new(specimen.Ownership.TrainerId);
      if (!specimen.Slot.Box.HasValue)
      {
        IEnumerable<PokemonId> memberIds = sourceRoster.GetParty().Except([specimen.Id]);
        IEnumerable<Specimen> members = (await _pokemonRepository.LoadAsync(memberIds, cancellationToken)).Concat([specimen]);

        party = new(members);
        party.EnsureIsValidWithout(specimen);
      }
      sourceRoster.Remove(specimen, party, userId);

      Location location = new(payload.Location);
      specimen.Gift(trainer, location, userId);

      Roster destinationRoster = await _rosterRepository.LoadAsync(trainer, cancellationToken);
      destinationRoster.Add(specimen, userId);

      await _pokemonRepository.SaveAsync(specimen, cancellationToken);
      await _rosterRepository.SaveAsync([sourceRoster, destinationRoster], cancellationToken);
    }

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
