using Logitar.EventSourcing;
using MassTransit;
using Moq;
using PokeGame.Core.Pokedexes;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Trainers;
using PokeGame.Core.Varieties;
using PokeGame.Infrastructure.Messaging.Consumers;

namespace PokeGame.Pokedexes;

public class RegisterPokedexEntryAcquiredConsumerTests : UnitTests
{
  private readonly Mock<IPokedexService> _pokedexService = new();
  private readonly RegisterPokedexEntryAcquiredConsumer _consumer;

  public RegisterPokedexEntryAcquiredConsumerTests()
  {
    _consumer = new RegisterPokedexEntryAcquiredConsumer(_pokedexService.Object);
  }

  [Fact(DisplayName = "It should register the acquired variety on the trainer pokedex.")]
  public async Task Given_PokemonAcquired_When_Consume_Then_RegisterEntryAcquired()
  {
    TrainerId trainerId = Catalog.Red.Id;
    VarietyId varietyId = Catalog.Variety.Id;
    PokemonAcquired message = new(trainerId, Catalog.CreatePokemon().Id, varietyId);
    ActorId actorId = new(Guid.NewGuid().ToString());

    await _consumer.Consume(CreateContext(message, actorId.Value));

    _pokedexService.Verify(x => x.RegisterEntryAcquiredAsync(trainerId, varietyId, actorId, CancellationToken.None), Times.Once);
  }

  [Fact(DisplayName = "It should register without an actor when the header is missing.")]
  public async Task Given_NoActorHeader_When_Consume_Then_RegisterWithoutActor()
  {
    PokemonAcquired message = new(Catalog.Red.Id, Catalog.CreatePokemon().Id, Catalog.Variety.Id);

    await _consumer.Consume(CreateContext(message, actorId: null));

    _pokedexService.Verify(x => x.RegisterEntryAcquiredAsync(Catalog.Red.Id, Catalog.Variety.Id, null, CancellationToken.None), Times.Once);
  }

  private static ConsumeContext<PokemonAcquired> CreateContext(PokemonAcquired message, string? actorId)
  {
    Mock<ConsumeContext<PokemonAcquired>> context = new();
    context.SetupGet(x => x.Message).Returns(message);
    context.SetupGet(x => x.CancellationToken).Returns(CancellationToken.None);

    Mock<Headers> headers = new();
    headers
      .Setup(x => x.TryGetHeader("ActorId", out It.Ref<object>.IsAny!))
      .Returns((string _, out object value) =>
      {
        if (actorId is null)
        {
          value = null!;
          return false;
        }

        value = actorId;
        return true;
      });
    context.SetupGet(x => x.Headers).Returns(headers.Object);

    return context.Object;
  }
}
