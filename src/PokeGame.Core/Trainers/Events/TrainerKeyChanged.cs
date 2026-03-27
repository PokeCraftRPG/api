using Logitar.EventSourcing;

namespace PokeGame.Core.Trainers.Events;

public record TrainerKeyChanged(Slug Key) : DomainEvent;
