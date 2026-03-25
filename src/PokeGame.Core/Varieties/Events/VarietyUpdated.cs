using Logitar.EventSourcing;

namespace PokeGame.Core.Varieties.Events;

public record VarietyUpdated : DomainEvent
{
  public Optional<Name>? Name { get; set; }
  public Optional<Genus>? Genus { get; set; }
  public Optional<Description>? Description { get; set; }

  public Optional<GenderRatio>? GenderRatio { get; set; }

  public bool? CanChangeForm { get; set; }

  public Optional<Url>? Url { get; set; }
  public Optional<Notes>? Notes { get; set; }
}
