using Logitar.EventSourcing;

namespace PokeGame.Core.Regions.Events;

public record RegionKeyChanged(Slug Key) : DomainEvent;
