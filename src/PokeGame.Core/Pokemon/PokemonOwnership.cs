using PokeGame.Core.Items;
using PokeGame.Core.Regions;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Pokemon;

public record PokemonOwnership(TrainerId TrainerId, ItemId PokeBallId, Level Level, Location Location, DateTime MetOn);
