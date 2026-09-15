using Logitar.EventSourcing;

namespace PokeGame.Core.Rosters.Events;

public sealed record RosterTagRemoved(Guid TagId) : DomainEvent;
