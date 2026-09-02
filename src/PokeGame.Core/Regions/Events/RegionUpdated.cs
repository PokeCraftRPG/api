using Logitar.EventSourcing;

namespace PokeGame.Core.Regions.Events;

public sealed record RegionUpdated(Name? Name, Summary? Summary, Content? Content) : DomainEvent;
