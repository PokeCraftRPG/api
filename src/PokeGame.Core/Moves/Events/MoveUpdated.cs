using Logitar.EventSourcing;

namespace PokeGame.Core.Moves.Events;

public record MoveUpdated : DomainEvent
{
  public Optional<Name>? Name { get; set; }
  public Optional<Description>? Description { get; set; }

  public Optional<Accuracy>? Accuracy { get; set; }
  public Optional<Power>? Power { get; set; }
  public PowerPoints? PowerPoints { get; set; }

  public Optional<Url>? Url { get; set; }
  public Optional<Notes>? Notes { get; set; }
}
