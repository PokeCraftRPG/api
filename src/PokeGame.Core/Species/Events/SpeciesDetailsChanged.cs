using Logitar.EventSourcing;

namespace PokeGame.Core.Species.Events;

public sealed record SpeciesDetailsChanged(Name? Name, Summary? Summary, Content? Content) : DomainEvent;
