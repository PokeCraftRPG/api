using Logitar.EventSourcing;

namespace PokeGame.Core.Items.Events;

public record ItemCreated(Slug Key) : DomainEvent;
