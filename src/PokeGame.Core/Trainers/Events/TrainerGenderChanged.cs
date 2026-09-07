using Logitar.EventSourcing;

namespace PokeGame.Core.Trainers.Events;

public sealed record TrainerGenderChanged(Gender? Gender) : DomainEvent;
