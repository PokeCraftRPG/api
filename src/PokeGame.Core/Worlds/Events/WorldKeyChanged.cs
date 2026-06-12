using Logitar.EventSourcing;

namespace PokeGame.Core.Worlds.Events;

public record WorldKeyChanged(Slug Key) : DomainEvent;
