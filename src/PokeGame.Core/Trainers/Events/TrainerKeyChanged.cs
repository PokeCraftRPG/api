using Logitar.EventSourcing;

namespace PokeGame.Core.Trainers.Events;

public sealed record TrainerKeyChanged(Key Key) : DomainEvent;
