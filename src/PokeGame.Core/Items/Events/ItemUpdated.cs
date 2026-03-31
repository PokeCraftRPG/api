using Logitar.EventSourcing;

namespace PokeGame.Core.Items.Events;

public record ItemUpdated : DomainEvent
{
  public Optional<Name>? Name { get; set; }
  public Optional<Description>? Description { get; set; }

  public Optional<Price>? Price { get; set; }

  public Optional<Url>? Sprite { get; set; }
  public Optional<Url>? Url { get; set; }
  public Optional<Notes>? Notes { get; set; }
}
