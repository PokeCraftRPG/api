using Logitar.EventSourcing;

namespace PokeGame.Core.Moves.Events;

public record MoveUpdated : DomainEvent
{
  public Name? Name { get; set; }
  public Optional<Description>? Description { get; set; }

  public Optional<Url>? Url { get; set; }
  public Optional<Notes>? Notes { get; set; }
}
