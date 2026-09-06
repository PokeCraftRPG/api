using Logitar.EventSourcing;

namespace PokeGame.Core.Forms.Events;

public sealed record FormKeyChanged(Key Key) : DomainEvent;
