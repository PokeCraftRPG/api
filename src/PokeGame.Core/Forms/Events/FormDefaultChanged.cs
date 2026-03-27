using Logitar.EventSourcing;

namespace PokeGame.Core.Forms.Events;

public record FormDefaultChanged(bool IsDefault) : DomainEvent;
