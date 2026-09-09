using Logitar.EventSourcing;

namespace PokeGame.Core.Items.Events;

public sealed record ItemDeleted : DomainEvent, IDeleteEvent;
