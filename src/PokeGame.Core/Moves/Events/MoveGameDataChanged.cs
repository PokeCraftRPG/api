using Logitar.EventSourcing;

namespace PokeGame.Core.Moves.Events;

public record MoveGameDataChanged(Accuracy? Accuracy, Power? Power, PowerPoints PowerPoints) : DomainEvent;
