using Logitar.EventSourcing;

namespace PokeGame.Core.Abilities.Events;

public record AbilityUpdated : DomainEvent
{
  public Optional<Name>? Name { get; set; }
  public Optional<Description>? Description { get; set; }

  public Optional<Url>? Url { get; set; }
  public Optional<Notes>? Notes { get; set; }
}
