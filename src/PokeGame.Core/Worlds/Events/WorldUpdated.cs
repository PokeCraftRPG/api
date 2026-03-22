using Logitar.EventSourcing;

namespace PokeGame.Core.Worlds.Events;

public record WorldUpdated : DomainEvent
{
  public Optional<Name>? Name { get; set; }
  public Optional<Description>? Description { get; set; }
}
