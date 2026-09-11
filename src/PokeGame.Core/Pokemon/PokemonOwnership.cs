using PokeGame.Core.Items;
using PokeGame.Core.Regions;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Pokemon;

public record PokemonOwnership(OwnershipEvent Event, TrainerId TrainerId, ItemId PokeBallId, Level MetLevel, Location MetAt, DateTime MetOn);
