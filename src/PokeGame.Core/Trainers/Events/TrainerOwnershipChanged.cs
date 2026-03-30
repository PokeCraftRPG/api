using Logitar.EventSourcing;

namespace PokeGame.Core.Trainers.Events;

public record TrainerOwnershipChanged(UserId? OwnerId) : DomainEvent;
