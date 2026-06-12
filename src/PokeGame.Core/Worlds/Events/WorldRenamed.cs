using Logitar.EventSourcing;

namespace PokeGame.Core.Worlds.Events;

public record WorldRenamed(Name? Name) : DomainEvent;
