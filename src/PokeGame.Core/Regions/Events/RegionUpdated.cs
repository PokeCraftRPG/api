using Logitar.EventSourcing;

namespace PokeGame.Core.Regions.Events;

public record RegionUpdated : DomainEvent
{
  public Optional<Name>? Name { get; set; }
  public Optional<Description>? Description { get; set; }

  public Optional<Url>? Url { get; set; }
  public Optional<Notes>? Notes { get; set; }
}
