using Logitar.EventSourcing;
using MassTransit;
using PokeGame.Core.Pokedexes;
using PokeGame.Core.Pokemon.Events;

namespace PokeGame.Infrastructure.Messaging.Consumers;

internal class RegisterPokedexEntryAcquiredConsumer : IConsumer<PokemonAcquired>
{
  private readonly IPokedexService _pokedexService;

  public RegisterPokedexEntryAcquiredConsumer(IPokedexService pokedexService)
  {
    _pokedexService = pokedexService;
  }

  public async Task Consume(ConsumeContext<PokemonAcquired> context)
  {
    PokemonAcquired @event = context.Message;
    ActorId? actorId = context.GetActorId();
    await _pokedexService.RegisterEntryAcquiredAsync(@event.TrainerId, @event.VarietyId, actorId, context.CancellationToken);
  }
}
