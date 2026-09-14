using PokeGame.Core.Moves;

namespace PokeGame.Core.Pokemon;

public sealed record PokemonMove(Level LearnedAtLevel, LearningMethod LearningMethod, bool IsMastered = false, int PowerPointUpgrades = 0);
