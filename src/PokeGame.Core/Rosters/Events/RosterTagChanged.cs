using Logitar.EventSourcing;

namespace PokeGame.Core.Rosters.Events;

public sealed record RosterTagChanged(Guid TagId, Tag Tag) : DomainEvent;
