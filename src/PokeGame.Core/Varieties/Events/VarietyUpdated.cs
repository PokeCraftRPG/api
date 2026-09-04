using Logitar.EventSourcing;

namespace PokeGame.Core.Varieties.Events;

public sealed record VarietyUpdated(
  Name? Name,
  Summary? Summary,
  Content? Content,
  bool CanChangeForm,
  GenderRatio? GenderRatio,
  Genus? Genus) : DomainEvent;
