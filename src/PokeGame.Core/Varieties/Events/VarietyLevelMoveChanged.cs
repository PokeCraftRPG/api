using Logitar.EventSourcing;
using PokeGame.Core.Moves;

namespace PokeGame.Core.Varieties.Events;

public record VarietyLevelMoveChanged(MoveId MoveId, Level Level) : DomainEvent;
