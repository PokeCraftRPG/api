using Logitar.EventSourcing;
using PokeGame.Core.Items;
using PokeGame.Core.Regions;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Pokemon.Events;

public sealed record PokemonTraded(TrainerId TrainerId, ItemId PokeBallId, Level Level, Location Location) : DomainEvent;
