using Logitar.EventSourcing;
using PokeGame.Core.Moves;

namespace PokeGame.Core.Pokemon.Events;

public sealed record PokemonMoveLearned(MoveId MoveId, Level Level, LearningMethod Method, bool AddToMoveset) : DomainEvent;
