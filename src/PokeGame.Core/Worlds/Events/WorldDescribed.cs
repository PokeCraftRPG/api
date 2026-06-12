using Logitar.EventSourcing;

namespace PokeGame.Core.Worlds.Events;

public record WorldDescribed(Description? Description) : DomainEvent;
