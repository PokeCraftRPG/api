using Logitar.EventSourcing;

namespace PokeGame.Core.Trainers.Events;

public record TrainerCreated(License License, Slug Key, TrainerGender Gender) : DomainEvent;
