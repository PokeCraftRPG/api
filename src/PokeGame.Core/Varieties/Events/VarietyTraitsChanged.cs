using Logitar.EventSourcing;

namespace PokeGame.Core.Varieties.Events;

public sealed record VarietyTraitsChanged(bool CanChangeForm, GenderRatio? GenderRatio, Genus? Genus) : DomainEvent;
