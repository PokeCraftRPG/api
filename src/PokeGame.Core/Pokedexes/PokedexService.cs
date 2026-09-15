using Logitar.CQRS;
using Logitar.EventSourcing;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Pokedexes.Commands;
using PokeGame.Core.Trainers;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Pokedexes;

public interface IPokedexService
{
  Task RegisterEntryAcquiredAsync(TrainerId trainerId, VarietyId varietyId, ActorId? actorId = null, CancellationToken cancellationToken = default);
}

internal class PokedexService : IPokedexService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IPokedexService, PokedexService>();
    services.AddTransient<IPokedexManager, PokedexManager>();
    services.AddTransient<ICommandHandler<RegisterPokedexEntryAcquiredCommand, Unit>, RegisterPokedexEntryAcquiredCommandHandler>();
  }

  private readonly ICommandBus _commandBus;

  public PokedexService(ICommandBus commandBus)
  {
    _commandBus = commandBus;
  }

  public async Task RegisterEntryAcquiredAsync(TrainerId trainerId, VarietyId varietyId, ActorId? actorId, CancellationToken cancellationToken)
  {
    RegisterPokedexEntryAcquiredCommand command = new(trainerId, varietyId, actorId);
    await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
