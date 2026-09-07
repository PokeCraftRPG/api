using Logitar.EventSourcing;

namespace PokeGame.Core.Trainers.Events;

public sealed record TrainerDeleted : DomainEvent, IDeleteEvent;
