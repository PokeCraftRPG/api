using Logitar.EventSourcing;

namespace PokeGame.Core.Moves.Events;

public record MoveCreated(PokemonType Type, MoveCategory Category, Name Name, PowerPoints PowerPoints) : DomainEvent;
