using Logitar.EventSourcing;

namespace PokeGame.Core.Regions.Events;

public sealed record RegionDetailsChanged(Name? Name, Summary? Summary, Content? Content) : DomainEvent;
