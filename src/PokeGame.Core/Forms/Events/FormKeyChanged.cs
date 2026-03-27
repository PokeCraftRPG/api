using Logitar.EventSourcing;

namespace PokeGame.Core.Forms.Events;

public record FormKeyChanged(Slug Key) : DomainEvent;
