using Logitar.EventSourcing;

namespace PokeGame.Core.Forms.Events;

public sealed record FormDeleted : DomainEvent, IDeleteEvent;
