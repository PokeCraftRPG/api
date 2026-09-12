using PokeGame.Core.Items;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Regions;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Pokemon;

public sealed record PokemonOwnership(OwnershipEvent Event, TrainerId TrainerId, ItemId PokeBallId, Level MetLevel, Location MetAt, DateTime MetOn)
{
  public static PokemonOwnership Caught(PokemonCaught @event) => new(OwnershipEvent.Caught, @event.TrainerId, @event.PokeBallId, @event.Level, @event.Location, @event.OccurredOn);
  public static PokemonOwnership Received(PokemonReceived @event) => new(OwnershipEvent.Received, @event.TrainerId, @event.PokeBallId, @event.Level, @event.Location, @event.OccurredOn);
  public static PokemonOwnership Traded(PokemonTraded @event) => new(OwnershipEvent.Traded, @event.TrainerId, @event.PokeBallId, @event.Level, @event.Location, @event.OccurredOn);
}
