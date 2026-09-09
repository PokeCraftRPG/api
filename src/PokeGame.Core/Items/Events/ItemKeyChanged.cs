using Logitar.EventSourcing;

namespace PokeGame.Core.Items.Events;

public sealed record ItemKeyChanged(Key Key) : DomainEvent;
