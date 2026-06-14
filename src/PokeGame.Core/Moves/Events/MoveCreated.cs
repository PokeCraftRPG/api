using Logitar.EventSourcing;
namespace PokeGame.Core.Moves.Events;
public record MoveCreated(Slug Key) : DomainEvent;