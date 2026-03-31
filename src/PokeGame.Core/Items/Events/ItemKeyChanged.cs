using Logitar.EventSourcing;

namespace PokeGame.Core.Items.Events;

public record ItemKeyChanged(Slug Key) : DomainEvent;
