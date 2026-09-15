using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Trainers;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Pokedexes.Commands;

internal record RegisterPokedexEntryAcquiredCommand(TrainerId TrainerId, VarietyId VarietyId, ActorId? ActorId) : ICommand;

internal class RegisterPokedexEntryAcquiredCommandHandler : ICommandHandler<RegisterPokedexEntryAcquiredCommand, Unit>
{
  private readonly IPokedexRepository _pokedexRepository;

  public RegisterPokedexEntryAcquiredCommandHandler(IPokedexRepository pokedexRepository)
  {
    _pokedexRepository = pokedexRepository;
  }

  public async Task<Unit> HandleAsync(RegisterPokedexEntryAcquiredCommand command, CancellationToken cancellationToken)
  {
    PokedexId pokedexId = new(command.TrainerId);
    Pokedex pokedex = await _pokedexRepository.LoadAsync(pokedexId, cancellationToken) ?? new(pokedexId);

    pokedex.RegisterAcquired(command.VarietyId, command.ActorId);

    await _pokedexRepository.SaveAsync(pokedex, cancellationToken);

    return Unit.Value;
  }
}
