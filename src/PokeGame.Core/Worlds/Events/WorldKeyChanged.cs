using Logitar.EventSourcing;

namespace PokeGame.Core.Worlds.Events;

public sealed record WorldKeyChanged(Key Key) : DomainEvent;
