using Logitar.EventSourcing;

namespace PokeGame.Core.Species.Events;

public record SpeciesUpdated : DomainEvent
{
  public Optional<Name>? Name { get; set; }

  public Optional<Url>? Url { get; set; }
  public Optional<Notes>? Notes { get; set; }
}
