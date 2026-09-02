using Logitar.EventSourcing;

namespace PokeGame.Core.Moves.Events;

public sealed record MoveCreated(PokemonType Type, MoveCategory Category, Key Key) : DomainEvent;
